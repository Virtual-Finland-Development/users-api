using System.Security.Claims;
using System.Security.Principal;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
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
}