
namespace VirtualFinland.UserAPI.Security.AccessRequirements;

public sealed class RequestAccessConfig
{
    public string HeaderName { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
}