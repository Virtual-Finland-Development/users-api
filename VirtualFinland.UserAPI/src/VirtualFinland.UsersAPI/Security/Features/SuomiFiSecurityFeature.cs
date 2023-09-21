using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Security.Models;
using JwksExtension = VirtualFinland.UserAPI.Helpers.Extensions.JwksExtension;

namespace VirtualFinland.UserAPI.Security.Features;

public class SuomiFiSecurityFeature : SecurityFeature
{
    public SuomiFiSecurityFeature(SecurityFeatureOptions configuration, ICacheRepository cacheRepository) : base(configuration, cacheRepository) { }

    protected override void ConfigureOpenIdConnect(AuthenticationBuilder authentication)
    {
        authentication.AddJwtBearer(GetSecurityPolicySchemeName(), c =>
        {
            c.RequireHttpsMetadata = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "local"; // @TODO: Use EnvironmentExtensions
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

    protected override void LoadOpenIdConfigUrl()
    {
        // Skip loading the OpenID configuration URL as it's already resolved in the constructor
    }
}