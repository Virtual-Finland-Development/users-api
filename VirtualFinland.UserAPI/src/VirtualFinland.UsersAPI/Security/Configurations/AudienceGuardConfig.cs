namespace VirtualFinland.UserAPI.Security.Configurations;

public class AudienceGuardConfig
{
    public AudienceGuardStaticConfig StaticConfig { get; set; } = default!;
    public AudienceGuardServiceConfig Service { get; set; } = default!;
}