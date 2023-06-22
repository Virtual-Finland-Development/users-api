using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;

namespace VirtualFinland.UserAPI.Helpers.Security.Features;

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
            c.RequireHttpsMetadata = true; // !EnvironmentExtensions.IsLocal(builder.Environment);
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
}