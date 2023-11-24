using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Security.Models;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;

namespace VirtualFinland.UserAPI.Security.Features;

public abstract class SecurityFeature : ISecurityFeature
{
    ///
    /// The issuer of the JWT token
    ///
    public string Issuer => _issuer ?? throw new ArgumentNullException("Issuer is not set");
    protected string? _issuer;

    /// <summary>
    /// Is the security feature initialized
    /// </summary>
    public bool IsInitialized { get; set; } = false;

    /// <summary>
    /// Security feature options
    /// </summary>
    protected SecurityFeatureOptions Options { get; set; }

    /// <summary>
    /// Cache repository
    /// </summary>
    protected SecurityClientProviders SecurityClientProviders { get; set; }

    protected ICacheRepository CacheRepository;

    /// <summary>
    /// The URL to the OpenID configuration
    /// </summary>
    protected string? _openIDConfigurationURL;

    /// <summary>
    /// The URL to the JWKS options (retrieved from the OpenID configuration)
    /// </summary>
    protected string? _jwksOptionsUrl;

    /// <summary>
    /// How many times to retry retrieving the openid configs
    /// </summary>
    protected const int _configUrlMaxRetryCount = 5;

    /// <summary>
    /// How long to wait in milliseconds before trying again to retrieve the openid configs
    /// </summary>
    protected const int _configUrlRetryWaitTime = 3000;

    public SecurityFeature(SecurityFeatureOptions options, SecurityClientProviders securityClientProviders)
    {
        Options = options;
        _issuer = options.Issuer;
        _openIDConfigurationURL = options.OpenIdConfigurationUrl;
        _jwksOptionsUrl = options.AuthorizationJwksJsonUrl;

        if (string.IsNullOrEmpty(_openIDConfigurationURL) && string.IsNullOrEmpty(_jwksOptionsUrl))
        {
            throw new ArgumentException("Invalid security feature configuration");
        }

        SecurityClientProviders = securityClientProviders;
        CacheRepository = SecurityClientProviders.CacheRepositoryFactory.Create(GetType().Name);
    }

    /// <summary>
    /// Builds the authentication configurations
    /// </summary>
    public void BuildAuthentication(AuthenticationBuilder authentication)
    {
        LoadOpenIdConfigUrl();
        ConfigureOpenIdConnect(authentication);
    }

    /// <summary>
    /// Builds the authorization configurations
    /// </summary>
    public void BuildAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(GetSecurityPolicySchemeName(), policy =>
        {
            policy.AuthenticationSchemes.Add(GetSecurityPolicySchemeName());
            policy.RequireAuthenticatedUser();
        });
    }

    // <summary>
    /// Returns the name of the security policy scheme (eg. SinunaSecurityFeatureScheme)
    /// </summary>
    public string GetSecurityPolicySchemeName()
    {
        return $"{GetType().Name}Scheme";
    }

    /// <summary>
    /// Resolves the user id from the JWT token
    /// </summary>
    public virtual string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        return jwtSecurityToken.Subject; // the "sub" claim
    }

    /// <summary>
    /// Validates the token audience
    /// </summary>
    /// <param name="audience"></param>
    /// <exception cref="NotAuthorizedException"></exception>
    public virtual async Task ValidateSecurityTokenAudience(string audience)
    {
        if (Options.AudienceGuard.StaticConfig.IsEnabled)
        {
            await ValidateSecurityTokenAudienceByStaticConfiguration(audience);
        }

        if (Options.AudienceGuard.Service.IsEnabled)
        {
            await ValidateSecurityTokenAudienceByService(audience);
        }
    }

    /// <summary>
    /// Validates the token audience by static configuration
    /// </summary>
    /// <param name="audience"></param>
    /// <exception cref="NotAuthorizedException"></exception>
    public virtual Task ValidateSecurityTokenAudienceByStaticConfiguration(string audience)
    {
        if (!Options.AudienceGuard.StaticConfig.AllowedAudiences.Contains(audience)) throw new NotAuthorizedException("The given token audience is not allowed");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates the token audience by external service
    /// Implement this in the derived class if using the audience guard service feature
    /// </summary>
    /// <param name="audience"></param>
    /// <exception cref="NotAuthorizedException"></exception>
    public virtual Task ValidateSecurityTokenAudienceByService(string audience)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Configures the OpenID Connect authentication
    /// </summary>
    protected virtual void ConfigureOpenIdConnect(AuthenticationBuilder authentication)
    {
        authentication.AddJwtBearer(GetSecurityPolicySchemeName(), c =>
        {
            JwksExtension.SetJwksOptions(c, new JwkOptions(_jwksOptionsUrl));

            c.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateActor = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer
            };
        });
    }

    /// <summary>
    /// Loads the OpenID configuration URL
    /// </summary>
    protected virtual async void LoadOpenIdConfigUrl()
    {
        if (_openIDConfigurationURL == null)
        {
            IsInitialized = true;
            return;
        }

        if (Options.IsOidcMetadataCachingEnabled && await CacheRepository.Exists(Constants.Cache.OpenIdConfigPrefix))
        {
            var cachedResult = await CacheRepository.Get<OpenIdConfiguration>(Constants.Cache.OpenIdConfigPrefix);
            _issuer = cachedResult.Issuer;
            _jwksOptionsUrl = cachedResult.JwksUri;
            IsInitialized = true;
            return;
        }

        var httpClient = SecurityClientProviders.HttpClient;
        var httpResponse = await httpClient.GetAsync(_openIDConfigurationURL);

        for (int retryCount = 0; retryCount < _configUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonData = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync());
                _issuer = jsonData?["issuer"]?.ToString();
                _jwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();

                if (!string.IsNullOrEmpty(_issuer) && !string.IsNullOrEmpty(_jwksOptionsUrl))
                {
                    if (Options.IsOidcMetadataCachingEnabled)
                    {
                        // Check for standard cache headers or use default cache duration
                        var cacheControlHeader = httpResponse.Headers.CacheControl;
                        var cacheDuration = cacheControlHeader?.MaxAge ?? TimeSpan.FromSeconds(Options.DefaultOidcMetadataCacheDurationInSeconds);

                        await CacheRepository.Set(Constants.Cache.OpenIdConfigPrefix, new OpenIdConfiguration()
                        {
                            Issuer = _issuer,
                            JwksUri = _jwksOptionsUrl
                        }, cacheDuration);

                        IsInitialized = true;
                    }

                    break;
                }
            }
            await Task.Delay(_configUrlRetryWaitTime);
        }

        // If all retries fail, then send an exception since the security information is critical to the functionality of the backend
        if (string.IsNullOrEmpty(_issuer) || string.IsNullOrEmpty(_jwksOptionsUrl))
        {
            throw new ArgumentNullException("Failed to retrieve OpenID configurations");
        }
    }

    private class OpenIdConfiguration
    {
        public string Issuer { get; set; } = default!;
        public string JwksUri { get; set; } = default!;
    }
}