using VirtualFinland.UserAPI.Security.Configurations;

namespace VirtualFinland.UserAPI.Security.Extensions;

public static class ConsentServiceProviderExtensions
{
    /// <summary>
    ///     Configure security features using appsettings.json
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterConsentServiceProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var testBedConsentProviderConfig = new TestBedConsentProviderConfig(configuration);
        services.AddSingleton<IConsentProviderConfig>(testBedConsentProviderConfig);

        if (configuration.GetValue<bool>("Security:Authorization:Testbed:IsEnabled"))
        {
            testBedConsentProviderConfig.LoadPublicKeys();
        }

        return services;
    }
}