namespace VirtualFinland.UserAPI.Security.Configurations;

public class AudienceGuardServiceConfig
{
    public string ApiEndpoint { get; init; } = default!;
    public List<string> AllowedGroups { get; init; } = default!;
    public bool IsEnabled { get; init; }
}