using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security.Features;

public class AccessFinlandSecurityFeature : SecurityFeature
{
    public AccessFinlandSecurityFeature(SecurityFeatureOptions configuration, SecurityClientProviders securityClientProviders) : base(configuration, securityClientProviders) { }
}