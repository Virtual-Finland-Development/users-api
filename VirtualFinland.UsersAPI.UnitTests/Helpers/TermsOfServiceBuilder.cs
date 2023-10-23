using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

internal class TermsOfServiceBuilder
{
    public static TermsOfService Build()
    {
        return new TermsOfService
        {
            Url = "https://test.localhost/terms-of-service",
            Version = "1.0.0",
            Description = "Test terms of service"
        };
    }
}