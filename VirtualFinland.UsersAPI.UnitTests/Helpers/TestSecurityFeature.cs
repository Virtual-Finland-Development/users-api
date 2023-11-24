using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class TestSecurityFeature : SecurityFeature
{
    public TestSecurityFeature(SecurityFeatureOptions options, SecurityClientProviders securityClientProviders) : base(options, securityClientProviders)
    {
        IsInitialized = true;
    }

    /// <summary>
    /// Validates the token audience by external service
    /// </summary>
    /// <param name="audience"></param>
    /// <exception cref="NotAuthorizedException"></exception>
    public override async Task ValidateSecurityTokenAudienceByService(string audience)
    {
        var verifyUrl = $"{Options.AudienceGuard.Service.ApiEndpoint}?audience={audience}";
        using var response = await SecurityClientProviders.HttpClient.GetAsync(verifyUrl);
        response.EnsureSuccessStatusCode();
    }
}