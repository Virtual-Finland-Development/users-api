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
using VirtualFinland.UserAPI.Security.Features;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using VirtualFinland.UserAPI.Security.Models;

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
        var features = new List<ISecurityFeature>();
        var applicationSecurity = new ApplicationSecurity(features);
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
        var features = new List<ISecurityFeature>();
        var applicationSecurity = new ApplicationSecurity(features);
        var userSecurityService = new UserSecurityService(_dbContext, mockLogger.Object, applicationSecurity);
        var authenticationService = new AuthenticationService(userSecurityService);
        // Act
        var act = () => authenticationService.GetCurrentUserId(mockHttpRequest.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }

    [Fact]
    public async Task Should_SuccessInAuthVerification()
    {
        // Arrange
        var dbEntity = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<UserSecurityService>>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { });
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Create mock jwt token
        var idToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            dbEntity.externalIdentity.Issuer,
            "test-audience",
            new List<Claim>
            {
                new Claim("sub", dbEntity.externalIdentity.IdentityId ?? throw new Exception("IdentityId is null")),
            },
            DateTime.Now,
            DateTime.Now.AddDays(1),
            new SigningCredentials(new SymmetricSecurityKey(new byte[16]), SecurityAlgorithms.HmacSha256)
        ));

        mockHeaders.Setup(o => o.Authorization).Returns($"Bearer {idToken}");
        mockHttpClientFactory.Setup(o => o.CreateClient(It.IsAny<string>())).Returns(httpClient);
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);

        var mockConfiguration = new Mock<IConfiguration>();
        var features = new List<ISecurityFeature>
        {
            new SecurityFeature(new SecurityFeatureOptions { Issuer = dbEntity.externalIdentity.Issuer, OpenIdConfigurationUrl = "test-openid-config-url" })
        };

        var applicationSecurity = new ApplicationSecurity(features);
        var userSecurityService = new UserSecurityService(_dbContext, mockLogger.Object, applicationSecurity);
        var authenticationService = new AuthenticationService(userSecurityService);

        // Act
        var result = await authenticationService.GetCurrentUserId(mockHttpRequest.Object);

        // Assert
        result.Should().Be(dbEntity.user.Id);
    }
}
