namespace VirtualFinland.UserAPI.Security.Models;

public class SecurityFeatureOptions
{
    public string? OpenIdConfigurationUrl { get; set; }
    public string? AuthorizationJwksJsonUrl { get; set; }
    public string? Issuer { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public List<string> AllowedAudiences { get; set; } = new List<string>();
    public bool AudienceGuardEnabled { get; set; } = false;
}