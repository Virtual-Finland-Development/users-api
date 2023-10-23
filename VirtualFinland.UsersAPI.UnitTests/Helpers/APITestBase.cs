using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Amazon.CloudWatch;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security;
using VirtualFinland.UserAPI.Security.Configurations;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class APITestBase
{
    protected readonly UsersDbContext _dbContext;

    public APITestBase()
    {
        _dbContext = GetMemoryContext();
    }

    private UsersDbContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
            .Options;
        return new UsersDbContext(options, true);
    }

    public (IRequestAuthenticationCandinate requestAuthenticationCandinate, AuthenticationService authenticationService, Mock<HttpContext> httpContext) GetGoodLoginRequestSituation(IRequestAuthenticationCandinate requestAuthenticationCandinate)
    {
        // Create mock jwt token
        var idToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            requestAuthenticationCandinate.Issuer,
            requestAuthenticationCandinate.Audience,
            new List<Claim>
            {
                new("sub", requestAuthenticationCandinate.IdentityId ?? throw new Exception("IdentityId is null")),
            },
            DateTime.Now,
            DateTime.Now.AddDays(1),
            new SigningCredentials(new SymmetricSecurityKey(new byte[16]), SecurityAlgorithms.HmacSha256)
        ));

        var analyticsServiceFactoryMock = GetMockedAnalyticsServiceFactory();
        var securityClientProviders = new SecurityClientProviders()
        {
            HttpClient = new Mock<HttpClient>().Object,
            CacheRepositoryFactory = new Mock<ICacheRepositoryFactory>().Object,
        };

        var applicationSecurity = new ApplicationSecurity(
            new TermsOfServiceRepository(GetMockedServiceProvider().Object),
            new SecuritySetup()
            {
                Features = new List<ISecurityFeature>() {
                    new SecurityFeature(
                        new SecurityFeatureOptions {
                            Issuer = requestAuthenticationCandinate.Issuer,
                            OpenIdConfigurationUrl = "test-openid-config-url",
                            AudienceGuard = new AudienceGuardConfig {
                                StaticConfig = new AudienceGuardStaticConfig {
                                    IsEnabled = true,
                                    AllowedAudiences = new List<string> { requestAuthenticationCandinate.Audience }
                                },
                                Service = new AudienceGuardServiceConfig {
                                    IsEnabled = false
                                }
                            }
                        },
                        securityClientProviders
                    )
                },
                Options = new SecurityOptions()
                {
                    TermsOfServiceAgreementRequired = false
                }
            }
        );

        var authenticationService = new AuthenticationService(_dbContext, analyticsServiceFactoryMock, applicationSecurity);
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHeaders.Setup(o => o.Authorization).Returns($"Bearer {idToken}");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);
        mockHttpContext.Setup(o => o.Request).Returns(mockHttpRequest.Object);
        mockHttpContext.Setup(o => o.Items).Returns(new Dictionary<object, object?>());

        return (requestAuthenticationCandinate, authenticationService, mockHttpContext);
    }

    protected Mock<IServiceProvider> GetMockedServiceProvider()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(UsersDbContext)))
            .Returns(_dbContext);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);

        serviceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        return serviceProvider;
    }

    protected AnalyticsServiceFactory GetMockedAnalyticsServiceFactory()
    {
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(o => o.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
        var cloudWatchClient = new Mock<IAmazonCloudWatch>();
        var analyticsConfig = Options.Create(new AnalyticsConfig()
        {
            CloudWatch = new AnalyticsConfig.CloudWatchSettings()
            {
                IsEnabled = true,
                Namespace = "test-namespace"
            }
        });

        return new AnalyticsServiceFactory(analyticsConfig, loggerFactory.Object, cloudWatchClient.Object);
    }
}