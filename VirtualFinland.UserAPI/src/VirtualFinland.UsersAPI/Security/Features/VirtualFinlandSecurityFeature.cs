using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security.Features;

public class VirtualFinlandSecurityFeature : SecurityFeature
{
    public VirtualFinlandSecurityFeature(SecurityFeatureOptions configuration, SecurityClientProviders securityClientProviders) : base(configuration, securityClientProviders) { }
}