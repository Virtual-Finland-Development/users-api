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
        var mockLogger = new Mock<ILogger<AuthenticationService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "AuthGW:Issuer")]).Returns("IssuerIdentity");
        var mockHttpContext = new Mock<HttpContext>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
        var mockUserIdentity = new Mock<IIdentity>();
        IList<Claim> invalidClaims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, "user id not a match  from a token compared to database"),
            new Claim("issuer", "issuer not a match from a token compared to database")
        };
        
        mockHttpContext.Setup(o => o.Request).Returns(mockHttpRequest.Object);
        mockHttpContext.Setup(o => o.Request.Path).Returns("/some-path");
        mockHttpContext.Setup(o => o.User).Returns(mockClaimsPrincipal.Object);
        mockHttpContext.Setup(o => o.User.Identity).Returns(mockUserIdentity.Object);
        mockHttpContext.Setup(o => o.User.Identity.IsAuthenticated).Returns(true);
        mockHttpContext.Setup(o => o.User.Claims).Returns(invalidClaims);
        mockHttpRequest.Setup(o => o.HttpContext).Returns(mockHttpContext.Object);
        
        var authenticationService = new AuthenticationService(_dbContext, mockLogger.Object, mockConfiguration.Object);

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
        var mockConfiguration = new Mock<IConfiguration>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {});
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "AuthGW:AuthorizeURL")]).Returns("https://someurl.com");
        mockHeaders.Setup(o => o.Authorization).Returns("");
        mockHttpClientFactory.Setup(o => o.CreateClient(It.IsAny<string>())).Returns(httpClient);
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);

        var authGwVerificationService = new AuthGwVerificationService(mockLogger.Object, mockConfiguration.Object, mockHttpClientFactory.Object, _dbContext);

        // Act
        var act = () => authGwVerificationService.AuthGwVerification(mockHttpRequest.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }
}