namespace VirtualFinland.UserAPI.Security.Configurations;

public class AudienceGuardConfig
{
    public AudienceGuardStaticConfig StaticConfig { get; set; } = new();
    public AudienceGuardServiceConfig Service { get; set; } = new();
}