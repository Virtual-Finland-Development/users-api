using FluentAssertions;
using Moq;
using VirtualFinland.UserAPI.Security;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UserAPI.Security.Configurations;
using VirtualFinland.UserAPI.Helpers;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Moq.Protected;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Security;

public class TimeoutTests : APITestBase
{

    [Fact]
    ///
    /// Test that the security feature times out when retrieving service information using the AudienceGuardService feature
    /// 
    public async Task Should_RetrievingSecurityServiceInformation_shouldThrowTimeoutException()
    {
        // Arrange
        var requestDelay = 2;
        var requestTimeout = 1;

        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(requestDelay);
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("dummy")
                };
            });
        var httpClient = new HttpClient(new HttpRequestTimeoutHandler
        {
            DefaultTimeout = TimeSpan.FromMilliseconds(requestTimeout),
            DefaultTimeoutMessage = "Security feature request timeout",
            InnerHandler = new HttpClientHandler()
        });

        var securityClientProviders = new SecurityClientProviders()
        {
            HttpClient = httpClient,
            CacheRepositoryFactory = new Mock<ICacheRepositoryFactory>().Object,
        };

        var applicationSecurity = new ApplicationSecurity(
            new TermsOfServiceRepository(GetMockedServiceProvider().Object),
            new SecuritySetup()
            {
                Features = new List<ISecurityFeature>() {
                    new TestSecurityFeature(
                        new SecurityFeatureOptions {
                            Issuer = "dummy",
                            OpenIdConfigurationUrl = "http://dummy",
                            AudienceGuard = new AudienceGuardConfig {
                                StaticConfig = new AudienceGuardStaticConfig {
                                    IsEnabled = false,
                                    AllowedAudiences = new List<string> { "dummy" }
                                },
                                Service = new AudienceGuardServiceConfig {
                                    IsEnabled = true,
                                    ApiEndpoint = "http://dummy",
                                    AllowedGroups = new List<string> { "dummy" }
                                }
                            }
                        },
                        securityClientProviders
                    )
                },
                Options = new SecurityOptions()
                {
                    TermsOfServiceAgreementRequired = false,
                    ServiceRequestTimeoutInMilliseconds = 1
                }
            }
        );

        // Create mock jwt token
        var idToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            "dummy",
            "dummy",
            new List<Claim>
            {
                new("sub", "dummy"),
            },
            DateTime.Now,
            DateTime.Now.AddDays(1),
            new SigningCredentials(new SymmetricSecurityKey(new byte[16]), SecurityAlgorithms.HmacSha256)
        ));

        // Act
        var act = () => applicationSecurity.ParseJwtToken(idToken);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>();
    }
}
