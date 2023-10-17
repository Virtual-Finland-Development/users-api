using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security.Features;

public class TestbedSecurityFeature : SecurityFeature
{
    public TestbedSecurityFeature(SecurityFeatureOptions configuration, SecurityClientProviders securityClientProviders) : base(configuration, securityClientProviders) { }
}