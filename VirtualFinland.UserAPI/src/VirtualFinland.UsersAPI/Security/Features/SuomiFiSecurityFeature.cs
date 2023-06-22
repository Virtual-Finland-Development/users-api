using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using VirtualFinland.UserAPI.Helpers;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;

namespace VirtualFinland.UserAPI.Security.Features;

public class SuomiFiSecurityFeature : ISecurityFeature
{
    public string? JwksOptionsUrl { get; }

    public string? Issuer { get; }

    public SuomiFiSecurityFeature(IConfiguration configuration)
    {
        JwksOptionsUrl = configuration["SuomiFi:AuthorizationJwksJsonUrl"];
        Issuer = configuration["SuomiFi:Issuer"];
    }

    public void BuildAuthentication(AuthenticationBuilder authentication)
    {
        authentication.AddJwtBearer(Constants.Security.SuomiFiBearerScheme, c =>
        {
            c.RequireHttpsMetadata = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "local"; // @TODO: Use EnvironmentExtensions
            JwksExtension.SetJwksOptions(c, new JwkOptions(JwksOptionsUrl));
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

    public void BuildAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(Constants.Security.SuomiFiBearerScheme, policy =>
        {
            policy.AuthenticationSchemes.Add(Constants.Security.SuomiFiBearerScheme);
            policy.RequireAuthenticatedUser();
        });
    }

    public string GetSecurityPolicyScheme()
    {
        return Constants.Security.TestBedBearerScheme;
    }

    public string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        return jwtSecurityToken.Subject; // the "sub" claim
    }
}