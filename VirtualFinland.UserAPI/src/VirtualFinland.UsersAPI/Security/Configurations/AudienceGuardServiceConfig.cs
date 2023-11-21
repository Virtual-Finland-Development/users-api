namespace VirtualFinland.UserAPI.Security.Configurations;

public class AudienceGuardServiceConfig
{
    public string ApiEndpoint { get; init; } = string.Empty;
    public List<string> AllowedGroups { get; init; } = new();
    public bool IsEnabled { get; init; } = false;
}