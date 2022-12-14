using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VirtualFinland.UserAPI.Exceptions;
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
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "AuthGW:Issuer")]).Returns("IssuerIdentity");
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var UserSecurityService = new UserSecurityService(_dbContext, mockUserSecurityLogger.Object, mockConfiguration.Object);

        mockHeaders.Setup(o => o.Authorization).Returns("");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);
        
        var authenticationService = new AuthenticationService(UserSecurityService);

        // Act
        var act = () => authenticationService.GetCurrentUserId(mockHttpRequest.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }
    
    [Fact]
    public async Task Should_FailAuthGwVerificationIfEmptyToken()
    {
        // Arrange
        await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<AuthGwVerificationService>>();
        var mockUserSecurityLogger = new Mock<ILogger<UserSecurityService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()  
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {});
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var UserSecurityService = new UserSecurityService(_dbContext, mockUserSecurityLogger.Object, mockConfiguration.Object);

        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "AuthGW:AuthorizeURL")]).Returns("https://someurl.com");
        mockHeaders.Setup(o => o.Authorization).Returns("");
        mockHttpClientFactory.Setup(o => o.CreateClient(It.IsAny<string>())).Returns(httpClient);
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);

        var authGwVerificationService = new AuthGwVerificationService(mockLogger.Object, mockConfiguration.Object, mockHttpClientFactory.Object, UserSecurityService);

        // Act
        var act = () => authGwVerificationService.AuthGwVerification(mockHttpRequest.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }
}