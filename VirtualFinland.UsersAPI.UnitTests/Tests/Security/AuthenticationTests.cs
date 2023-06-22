using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UsersAPI.UnitTests.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Security;

public class AuthenticationTests : APITestBase
{
    [Fact]
    public async Task Should_FailIfTokenClaimsNotInUsersDb()
    {
        // Arrange
        await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockUserSecurityLogger = new Mock<ILogger<UserSecurityService>>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockConfiguration = new Mock<IConfiguration>();
        var applicationSecurity = new ApplicationSecurity(mockConfiguration.Object);
        var userSecurityService = new UserSecurityService(_dbContext, mockUserSecurityLogger.Object, applicationSecurity);

        mockHeaders.Setup(o => o.Authorization).Returns("");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);

        var authenticationService = new AuthenticationService(userSecurityService);

        // Act
        var act = () => authenticationService.GetCurrentUserId(mockHttpRequest.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }

    [Fact]
    public async Task Should_FailAuthVerificationIfEmptyToken()
    {
        // Arrange
        await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UserSecurityService>>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { });
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        mockHeaders.Setup(o => o.Authorization).Returns("");
        mockHttpClientFactory.Setup(o => o.CreateClient(It.IsAny<string>())).Returns(httpClient);
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);

        var mockConfiguration = new Mock<IConfiguration>();
        var applicationSecurity = new ApplicationSecurity(mockConfiguration.Object);
        var userSecurityService = new UserSecurityService(_dbContext, mockLogger.Object, applicationSecurity);
        var authenticationService = new AuthenticationService(userSecurityService);
        // Act
        var act = () => authenticationService.GetCurrentUserId(mockHttpRequest.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }
}
