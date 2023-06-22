using System.IdentityModel.Tokens.Jwt;
using Microsoft.Net.Http.Headers;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Security.Features;

namespace VirtualFinland.UserAPI.Helpers.Security;

public class ApplicationSecuritySetup : IApplicationSecuritySetup
{
    private readonly IConfiguration _configuration;

    public readonly ISecurityFeature testbedSecurityFeature;
    public readonly IConsentProviderConfig testBedConsentProviderConfig;
    public readonly ISecurityFeature sinunaSecurityFeature;
    public readonly ISecurityFeature suomiFiSecurityFeature;

    public ApplicationSecuritySetup(IConfiguration configuration)
    {
        _configuration = configuration;

        testbedSecurityFeature = new TestbedSecurityFeature(configuration);
        testBedConsentProviderConfig = new TestBedConsentProviderConfig(configuration);
        sinunaSecurityFeature = new SinunaSecurityFeature(configuration);
        suomiFiSecurityFeature = new SuomiFiSecurityFeature(configuration);
    }

    public void BuildSecurity(WebApplicationBuilder builder)
    {
        // @see: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-6.0
        var authentication = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
            options.DefaultChallengeScheme = Constants.Security.ResolvePolicyFromTokenIssuer;
        });

        testbedSecurityFeature.BuildAuthentication(authentication);
        testBedConsentProviderConfig.LoadPublicKeys();
        sinunaSecurityFeature.BuildAuthentication(authentication);
        suomiFiSecurityFeature.BuildAuthentication(authentication);

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
                            case var value when value == testbedSecurityFeature.Issuer:
                                return Constants.Security.TestBedBearerScheme;
                            case var value when value == sinunaSecurityFeature.Issuer:
                                return Constants.Security.SinunaScheme;
                            case var value when value == suomiFiSecurityFeature.Issuer:
                                return Constants.Security.SuomiFiBearerScheme;
                        }
                    }
                }
                return Constants.Security.TestBedBearerScheme; // Defaults to testbed
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            testbedSecurityFeature.BuildAuthorization(options);
            sinunaSecurityFeature.BuildAuthorization(options);
            suomiFiSecurityFeature.BuildAuthorization(options);
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

    public string? GetTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        throw new NotImplementedException();
    }
}