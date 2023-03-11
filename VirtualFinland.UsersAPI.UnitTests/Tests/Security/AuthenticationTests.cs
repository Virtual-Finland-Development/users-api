using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
        var mockUserSecurityLogger = new Mock<ILogger<UserSecurityService>>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var userSecurityService = new UserSecurityService(_dbContext, mockUserSecurityLogger.Object);

        mockHeaders.Setup(o => o.Authorization).Returns("");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);

        var authenticationService = new AuthenticationService(userSecurityService);

        // Act
        var act = () => authenticationService.GetCurrentUserId(mockHttpRequest.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }
}
