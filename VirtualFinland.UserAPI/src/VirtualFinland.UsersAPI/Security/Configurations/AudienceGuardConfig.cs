namespace VirtualFinland.UserAPI.Security.Configurations;

public class AudienceGuardConfig
{
    public List<string> AllowedAudiences { get; set; } = new List<string>();
    public bool IsEnabled { get; set; } = false;
    public AudienceGuardServiceConfig? Service { get; set; }
}