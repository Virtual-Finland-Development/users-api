using System.IdentityModel.Tokens.Jwt;
using Microsoft.Net.Http.Headers;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Security.Features;

namespace VirtualFinland.UserAPI.Helpers.Security;

public class ApplicationSecurity : IApplicationSecurity
{
    private readonly IConfiguration _configuration;

    public readonly ISecurityFeature testbedSecurityFeature;
    public readonly IConsentProviderConfig testBedConsentProviderConfig;
    public readonly ISecurityFeature sinunaSecurityFeature;
    public readonly ISecurityFeature suomiFiSecurityFeature;

    public ApplicationSecurity(IConfiguration configuration)
    {
        _configuration = configuration;

        testbedSecurityFeature = new TestbedSecurityFeature(configuration);
        testBedConsentProviderConfig = new TestBedConsentProviderConfig(configuration);

        sinunaSecurityFeature = new SinunaSecurityFeature(configuration);
        suomiFiSecurityFeature = new SuomiFiSecurityFeature(configuration);
    }

    public ApplicationSecurity()
    {
    }

    public void BuildSecurity(WebApplicationBuilder builder)
    {
        // @see: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-6.0
        var authentication = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
            options.DefaultChallengeScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
        });

        if (IsEnabledSecurityFeature("Testbed"))
        {
            testbedSecurityFeature.BuildAuthentication(authentication);
            testBedConsentProviderConfig.LoadPublicKeys();
        }
        if (IsEnabledSecurityFeature("Sinuna"))
        {
            sinunaSecurityFeature.BuildAuthentication(authentication);
        }
        if (IsEnabledSecurityFeature("SuomiFi"))
        {
            suomiFiSecurityFeature.BuildAuthentication(authentication);
        }

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
                        switch (issuer)
                        {
                            // Cheers: https://stackoverflow.com/a/65642709
                            case var value when value == testbedSecurityFeature.Issuer && IsEnabledSecurityFeature("Testbed"):
                                return Constants.Security.TestBedBearerScheme;
                            case var value when value == sinunaSecurityFeature.Issuer && IsEnabledSecurityFeature("Sinuna"):
                                return Constants.Security.SinunaScheme;
                            case var value when value == suomiFiSecurityFeature.Issuer && IsEnabledSecurityFeature("SuomiFi"):
                                return Constants.Security.SuomiFiBearerScheme;
                        }
                    }
                }
                throw new NotAuthorizedException("Invalid token provided");
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            if (IsEnabledSecurityFeature("Testbed"))
            {
                testbedSecurityFeature.BuildAuthorization(options);
            }
            if (IsEnabledSecurityFeature("Sinuna"))
            {
                sinunaSecurityFeature.BuildAuthorization(options);
            }
            if (IsEnabledSecurityFeature("SuomiFi"))
            {
                suomiFiSecurityFeature.BuildAuthorization(options);
            }
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