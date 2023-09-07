using System.IdentityModel.Tokens.Jwt;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security.Features;

public class SinunaSecurityFeature : SecurityFeature
{
    public SinunaSecurityFeature(SecurityFeatureOptions configuration) : base(configuration) { }

    /// <summary>
    /// Resolves the user id (persistentId) from the Sinuna JWT token
    /// </summary>
    public override string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        var sinunaUserIdClaimType = "persistent_id";
        return jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == sinunaUserIdClaimType)?.Value ?? null;
    }
}