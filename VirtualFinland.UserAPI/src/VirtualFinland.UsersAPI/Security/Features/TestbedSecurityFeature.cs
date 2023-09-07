using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security.Features;

public class TestbedSecurityFeature : SecurityFeature
{
    public TestbedSecurityFeature(SecurityFeatureOptions configuration) : base(configuration) { }
}