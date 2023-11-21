namespace VirtualFinland.UserAPI.Security.Configurations;

public class AudienceGuardStaticConfig
{
    public List<string> AllowedAudiences { get; set; } = new();
    public bool IsEnabled { get; set; } = false;
}