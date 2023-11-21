using VirtualFinland.UserAPI.Security.Features;

namespace VirtualFinland.UserAPI.Security.Models;

public class SecuritySetup
{
    public SecurityOptions Options { get; set; } = new();
    public List<ISecurityFeature> Features { get; set; } = new();
}