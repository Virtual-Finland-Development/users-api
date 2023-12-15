using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UsersAPI.UnitTests.Helpers;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Security.Models;
using Amazon.SQS;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Security;

public class AuthenticationTests : APITestBase
{
    [Fact]
    public async Task Should_FailAuthVerificationIfEmptyToken()
    {
        // Arrange
        await APIUserFactory.CreateAndGetLogInUser(_dbContext);

        var AnalyticsLoggerFactoryMock = GetMockedAnalyticsLoggerFactory();
        var features = new List<ISecurityFeature>();
        var applicationSecurity = new ApplicationSecurity(new TermsOfServiceRepository(GetMockedServiceProvider().Object), new SecuritySetup() { Features = features, Options = new SecurityOptions() { TermsOfServiceAgreementRequired = false } });

        // Setup db event triggers mock
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(o => o.GetSection("Database:Triggers:SQS")).Returns(new Mock<IConfigurationSection>().Object);
        var activityTriggerService = new ActivityTriggerService(mockConfiguration.Object, new Mock<IAmazonSQS>().Object);

        var authenticationService = new AuthenticationService(_dbContext, AnalyticsLoggerFactoryMock, applicationSecurity, activityTriggerService);
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHeaders.Setup(o => o.Authorization).Returns("");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);
        mockHttpContext.Setup(o => o.Request).Returns(mockHttpRequest.Object);

        // Act
        var act = () => authenticationService.Authenticate(mockHttpContext.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }

    [Fact]
    public async Task Should_FailIfUnknownToken()
    {
        // Arrange
        await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHeaders.Setup(o => o.Authorization).Returns("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);
        mockHttpContext.Setup(o => o.Request).Returns(mockHttpRequest.Object);

        var mockConfiguration = new Mock<IConfiguration>();
        var features = new List<ISecurityFeature>();
        var applicationSecurity = new ApplicationSecurity(new TermsOfServiceRepository(GetMockedServiceProvider().Object), new SecuritySetup() { Features = features, Options = new SecurityOptions() { TermsOfServiceAgreementRequired = false } });
        var AnalyticsLoggerFactoryMock = GetMockedAnalyticsLoggerFactory();

        // Setup db event triggers mock
        mockConfiguration.Setup(o => o.GetSection("Database:Triggers:SQS")).Returns(new Mock<IConfigurationSection>().Object);
        var activityTriggerService = new ActivityTriggerService(mockConfiguration.Object, new Mock<IAmazonSQS>().Object);


        var authenticationService = new AuthenticationService(_dbContext, AnalyticsLoggerFactoryMock, applicationSecurity, activityTriggerService);

        // Act
        var act = () => authenticationService.Authenticate(mockHttpContext.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }

    [Fact]
    public async Task Should_SuccessInAuthVerification()
    {
        // Arrange
        var (person, _, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var (_, authenticationService, mockHttpContext) = GetGoodLoginRequestSituation(requestAuthenticatedUser);

        // Act
        var result = await authenticationService.Authenticate(mockHttpContext.Object);

        // Assert
        result.PersonId.Should().Be(person.Id);
    }
}
