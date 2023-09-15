using System.IdentityModel.Tokens.Jwt;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security.Features;

public class SinunaSecurityFeature : SecurityFeature
{
    private DataspaceAudienceSecurityService _dataspaceAudienceSecurityService;

    public SinunaSecurityFeature(SecurityFeatureOptions configuration) : base(configuration)
    {
        Console.WriteLine("SinunaSecurityFeature: " + configuration.AudienceGuard.IsEnabled);
        _dataspaceAudienceSecurityService = new DataspaceAudienceSecurityService(configuration.AudienceGuard.Service!);
    }

    /// <summary>
    /// Resolves the user id (persistentId) from the Sinuna JWT token
    /// </summary>
    public override string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        var sinunaUserIdClaimType = "persistent_id";
        return jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == sinunaUserIdClaimType)?.Value ?? null;
    }

    /// <summary>
    /// Validates the token audience
    /// </summary>
    /// <param name="audience"></param>
    /// <exception cref="NotAuthorizedException"></exception>
    public override async Task ValidateSecurityTokenAudience(string audience)
    {
        await _dataspaceAudienceSecurityService.VerifyAudience(audience);
    }
}