
namespace VirtualFinland.UserAPI.Security.AccessRequirements;

public sealed class RequestAccessConfig
{
    public string HeaderName { get; init; } = default!;
    public List<string> AccessKeys { get; init; } = default!;
    public bool IsEnabled { get; init; } = default!;
}