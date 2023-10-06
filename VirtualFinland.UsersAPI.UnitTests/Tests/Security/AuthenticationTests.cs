using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
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
    public async Task Should_FailAuthVerificationIfEmptyToken()
    {
        // Arrange
        await APIUserFactory.CreateAndGetLogInUser(_dbContext);

        var mockAuthenticationServiceLogger = new Mock<ILogger<AuthenticationService>>();
        var applicationSecurity = new ApplicationSecurity(new List<ISecurityFeature>());
        var authenticationService = new AuthenticationService(_dbContext, mockAuthenticationServiceLogger.Object, applicationSecurity);
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

        var mockAuthenticationServiceLogger = new Mock<ILogger<AuthenticationService>>();
        var applicationSecurity = new ApplicationSecurity(new List<ISecurityFeature>());
        var authenticationService = new AuthenticationService(_dbContext, mockAuthenticationServiceLogger.Object, applicationSecurity);
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHeaders.Setup(o => o.Authorization).Returns("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);
        mockHttpContext.Setup(o => o.Request).Returns(mockHttpRequest.Object);

        // Act
        var act = () => authenticationService.Authenticate(mockHttpContext.Object);

        // Assert
        await act.Should().ThrowAsync<NotAuthorizedException>();
    }

    [Fact]
    public async Task Should_SuccessInAuthVerification()
    {
        // Arrange
        var (user, externalIdentity) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);

        // Create mock jwt token
        var idToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            externalIdentity.Issuer,
            "test-audience",
            new List<Claim>
            {
                new("sub", externalIdentity.IdentityId ?? throw new Exception("IdentityId is null")),
            },
            DateTime.Now,
            DateTime.Now.AddDays(1),
            new SigningCredentials(new SymmetricSecurityKey(new byte[16]), SecurityAlgorithms.HmacSha256)
        ));

        var mockAuthenticationServiceLogger = new Mock<ILogger<AuthenticationService>>();
        var applicationSecurity = new ApplicationSecurity(new List<ISecurityFeature>
        {
            new SecurityFeature(new SecurityFeatureOptions { Issuer = externalIdentity.Issuer, OpenIdConfigurationUrl = "test-openid-config-url" })
        });
        var authenticationService = new AuthenticationService(_dbContext, mockAuthenticationServiceLogger.Object, applicationSecurity);
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHeaders.Setup(o => o.Authorization).Returns($"Bearer {idToken}");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);
        mockHttpContext.Setup(o => o.Request).Returns(mockHttpRequest.Object);

        // Act
        var result = await authenticationService.Authenticate(mockHttpContext.Object);

        // Assert
        result.Should().Be(user.Id);
    }
}
