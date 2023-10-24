using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class TestSecurityFeature : SecurityFeature
{
    public TestSecurityFeature(SecurityFeatureOptions options, SecurityClientProviders securityClientProviders) : base(options, securityClientProviders)
    {
    }

    /// <summary>
    /// Validates the token audience by external service
    /// </summary>
    /// <param name="audience"></param>
    /// <exception cref="NotAuthorizedException"></exception>
    public override async Task ValidateSecurityTokenAudienceByService(string audience)
    {
        var verifyUrl = $"{_options.AudienceGuard.Service.ApiEndpoint}?audience={audience}";
        using var response = await _securityClientProviders.HttpClient.GetAsync(verifyUrl);
        response.EnsureSuccessStatusCode();
    }
}