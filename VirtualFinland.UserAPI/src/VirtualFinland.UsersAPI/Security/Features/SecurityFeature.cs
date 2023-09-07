using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using VirtualFinland.UserAPI.Security.Models;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;

namespace VirtualFinland.UserAPI.Security.Features;

public class SecurityFeature : ISecurityFeature
{
    ///
    /// The issuer of the JWT token
    ///
    public string? Issuer { get; set; }

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

    public SecurityFeature(SecurityFeatureOptions configuration)
    {
        Issuer = configuration.Issuer;
        _openIDConfigurationURL = configuration.OpenIdConfigurationUrl;
        _jwksOptionsUrl = configuration.AuthorizationJwksJsonUrl;

        if (string.IsNullOrEmpty(_openIDConfigurationURL) && string.IsNullOrEmpty(_jwksOptionsUrl))
        {
            throw new ArgumentNullException("Invalid security feature configuration");
        }
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
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(_openIDConfigurationURL);

        for (int retryCount = 0; retryCount < _configUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonData = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync());
                Issuer = jsonData?["issuer"]?.ToString();
                _jwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();

                if (!string.IsNullOrEmpty(Issuer) && !string.IsNullOrEmpty(_jwksOptionsUrl))
                {
                    break;
                }
            }
            await Task.Delay(_configUrlRetryWaitTime);
        }

        // If all retries fail, then send an exception since the security information is critical to the functionality of the backend
        if (string.IsNullOrEmpty(Issuer) || string.IsNullOrEmpty(_jwksOptionsUrl))
        {
            throw new ArgumentNullException("Failed to retrieve OpenID configurations");
        }
    }
}