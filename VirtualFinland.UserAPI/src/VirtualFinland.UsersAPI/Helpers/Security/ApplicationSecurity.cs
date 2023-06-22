using System.IdentityModel.Tokens.Jwt;
using Microsoft.Net.Http.Headers;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Security.Features;

namespace VirtualFinland.UserAPI.Helpers.Security;

public class ApplicationSecurity : IApplicationSecurity
{
    private readonly IConfiguration _configuration;

    public readonly IConsentProviderConfig testBedConsentProviderConfig; // Custom case, @TODO: needs a refactor

    private readonly Dictionary<string, ISecurityFeature> _securityFeatures;

    public ApplicationSecurity(IConfiguration configuration)
    {
        _configuration = configuration;
        _securityFeatures = new Dictionary<string, ISecurityFeature>();

        // Initialize enabled security features
        if (IsEnabledSecurityFeature("Testbed"))
        {
            _securityFeatures["Testbed"] = new TestbedSecurityFeature(configuration);
        }
        if (IsEnabledSecurityFeature("Sinuna"))
        {
            _securityFeatures["Sinuna"] = new SinunaSecurityFeature(configuration);
        }
        if (IsEnabledSecurityFeature("SuomiFi"))
        {
            _securityFeatures["SuomiFi"] = new SuomiFiSecurityFeature(configuration);
        }

        // Custom case, @TODO: needs a refactor
        testBedConsentProviderConfig = new TestBedConsentProviderConfig(configuration);
    }

    public void BuildSecurity(WebApplicationBuilder builder)
    {
        // @see: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-6.0
        var authentication = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
            options.DefaultChallengeScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
        });

        // Initialize the authentication configurations of all enabled security features
        foreach (var securityFeature in _securityFeatures.Values)
        {
            securityFeature.BuildAuthentication(authentication);
        }

        // Custom case, @TODO: needs a refactor
        if (IsEnabledSecurityFeature("Testbed"))
        {
            testBedConsentProviderConfig.LoadPublicKeys();
        }

        // Initialize authorization policies for enabled security features
        builder.Services.AddAuthorization(options =>
        {
            foreach (var securityFeature in _securityFeatures.Values)
            {
                securityFeature.BuildAuthorization(options);
            }
        });

        // Configure application policy scheme to resolve the policy from the token issuer
        authentication.AddPolicyScheme(Constants.Security.ResolvePolicyFromTokenIssuer, Constants.Security.ResolvePolicyFromTokenIssuer, options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var authorizationValue = context.Request.Headers[HeaderNames.Authorization];
                var authorization = authorizationValue.FirstOrDefault();

                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    var token = authorization.Substring("Bearer ".Length).Trim();
                    var jwtHandler = new JwtSecurityTokenHandler();

                    if (jwtHandler.CanReadToken(token))
                    {
                        var issuer = jwtHandler.ReadJwtToken(token).Issuer;
                        foreach (var securityFeature in _securityFeatures.Values)
                        {
                            if (securityFeature.Issuer == issuer)
                            {
                                return securityFeature.GetSecurityPolicyScheme();
                            }
                        }
                    }
                }
                throw new NotAuthorizedException("Invalid token provided");
            };
        });
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id
    /// </summary>
    public JwtTokenResult ParseJWTToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new NotAuthorizedException("No token provided");
        }

        var issuer = GetTokenIssuer(token);
        var userId = GetTokenUserId(token);

        if (userId == null || issuer == null)
        {
            throw new NotAuthorizedException("The given token is not valid");
        }
        return new JwtTokenResult() { UserId = userId, Issuer = issuer };
    }

    private string? GetTokenUserId(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return string.Empty;
        }

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

        if (jwtSecurityToken == null)
        {
            return string.Empty;
        }

        if (jwtSecurityToken.Issuer == "https://login.iam.qa.sinuna.fi") // @TODO: read from SinunaIdentityProviderConfig
        {
            var claim = jwtSecurityToken.Claims.FirstOrDefault(o => o.Type == "persistent_id");
            return claim?.Value;
        }

        return string.IsNullOrEmpty(jwtSecurityToken.Subject) ? jwtSecurityToken.Claims.FirstOrDefault(o => o.Type == "userId")?.Value : jwtSecurityToken.Subject;
    }

    private string? GetTokenIssuer(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var canReadToken = tokenHandler.CanReadToken(token);
        return canReadToken ? tokenHandler.ReadJwtToken(token).Issuer : string.Empty;
    }

    private string? GetTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        throw new NotImplementedException();
    }

    private bool IsEnabledSecurityFeature(string featureName)
    {
        return _configuration.GetValue<bool>($"{featureName}:IsEnabled");
    }
}