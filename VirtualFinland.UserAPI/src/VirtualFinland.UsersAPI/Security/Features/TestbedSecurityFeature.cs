namespace VirtualFinland.UserAPI.Security.Features;

public class TestbedSecurityFeature : SecurityFeature
{
    public TestbedSecurityFeature(IConfiguration configuration)
    {
        _openIDConfigurationURL = configuration["Security:Configurations:Testbed:OpenIDConfigurationURL"];
    }
}