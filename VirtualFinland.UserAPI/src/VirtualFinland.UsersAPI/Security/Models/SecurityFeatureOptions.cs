using VirtualFinland.UserAPI.Security.Configurations;

namespace VirtualFinland.UserAPI.Security.Models;

public class SecurityFeatureOptions
{
    public string? OpenIdConfigurationUrl { get; set; }
    public string? AuthorizationJwksJsonUrl { get; set; }
    public string? Issuer { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public bool IsOidcMetadataCachingEnabled { get; set; }
    public int DefaultOidcMetadataCacheDurationInSeconds { get; set; }
    public AudienceGuardConfig AudienceGuard { get; set; } = new();
}