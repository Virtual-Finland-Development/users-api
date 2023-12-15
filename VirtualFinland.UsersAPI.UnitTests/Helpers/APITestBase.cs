using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Amazon.CloudWatch;
using Amazon.SQS;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.App;
using VirtualFinland.UserAPI.Models.UsersDatabase;
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

    public (IRequestAuthenticationCandinate requestAuthenticationCandinate, AuthenticationService authenticationService, Mock<HttpContext> httpContext) GetGoodLoginRequestSituation(IRequestAuthenticationCandinate requestAuthenticationCandinate, bool verifyTermsOfServiceAgreement = false)
    {
        // Create mock jwt token
        var idToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            requestAuthenticationCandinate.Issuer,
            requestAuthenticationCandinate.Audience,
            new List<Claim>
            {
                new("sub", requestAuthenticationCandinate.IdentityId ?? throw new Exception("IdentityId is null")),
            },
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            new SigningCredentials(new SymmetricSecurityKey(new byte[16]), SecurityAlgorithms.HmacSha256)
        ));

        var AnalyticsLoggerFactoryMock = GetMockedAnalyticsLoggerFactory();
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
                    new TestSecurityFeature(
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
                    TermsOfServiceAgreementRequired = verifyTermsOfServiceAgreement,
                    ServiceRequestTimeoutInMilliseconds = 1000
                }
            }
        );

        // Setup db event triggers mock
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(o => o.GetSection("Database:Triggers:SQS")).Returns(new Mock<IConfigurationSection>().Object);
        var activityTriggerService = new ActivityTriggerService(mockConfiguration.Object, new Mock<IAmazonSQS>().Object);

        var authenticationService = new AuthenticationService(_dbContext, AnalyticsLoggerFactoryMock, applicationSecurity, activityTriggerService);
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

    protected AnalyticsLoggerFactory GetMockedAnalyticsLoggerFactory()
    {
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(o => o.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        var cloudWatchClient = new Mock<IAmazonCloudWatch>();
        var sqsClient = new Mock<IAmazonSQS>();

        var analyticsConfig = new AnalyticsConfig(
            new CloudWatchSettings()
            {
                IsEnabled = true,
                Namespace = "test-namespace"
            },
            new SqsSettings()
            {
                IsEnabled = false,
                QueueUrl = "test-queue-url"
            }
        );

        var analyticsService = new AnalyticsService(analyticsConfig, cloudWatchClient.Object, sqsClient.Object);

        return new AnalyticsLoggerFactory(loggerFactory.Object, analyticsService);
    }

    protected async Task<TermsOfService> SetupTermsOfServices()
    {
        var termsOfServicesData = TermsOfServiceBuilder.Build();
        var tos = await _dbContext.TermsOfServices.SingleOrDefaultAsync(tos => tos.Version == termsOfServicesData.Version);
        if (tos != null)
        {
            return tos;
        }

        var entry = _dbContext.TermsOfServices.Add(termsOfServicesData);
        await _dbContext.SaveChangesAsync();
        return entry.Entity;
    }
}