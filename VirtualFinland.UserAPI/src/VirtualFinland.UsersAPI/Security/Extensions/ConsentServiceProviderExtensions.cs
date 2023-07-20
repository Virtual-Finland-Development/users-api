using VirtualFinland.UserAPI.Security.Features;

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
        var enabledSecurityFeatures = configuration.GetSection("Security:EnabledSecurityFeatures").Get<string[]>();
        var testBedConsentProviderConfig = new TestBedConsentProviderConfig(configuration);
        services.AddSingleton<IConsentProviderConfig>(testBedConsentProviderConfig);

        if (enabledSecurityFeatures.Contains("Testbed"))
        {
            testBedConsentProviderConfig.LoadPublicKeys();
        }

        return services;
    }
}