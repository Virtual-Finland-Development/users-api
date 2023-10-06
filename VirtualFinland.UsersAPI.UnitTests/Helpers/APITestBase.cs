using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Security;
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
        var auditInterceptor = new AuditInterceptor(new Mock<ILogger<IAuditInterceptor>>().Object);

        return new UsersDbContext(options, auditInterceptor, true);
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

        var mockAuthenticationServiceLogger = new Mock<ILogger<AuthenticationService>>();
        var applicationSecurity = new ApplicationSecurity(new List<ISecurityFeature>
        {
            new SecurityFeature(new SecurityFeatureOptions { Issuer = requestAuthenticationCandinate.Issuer, OpenIdConfigurationUrl = "test-openid-config-url" })
        });
        var authenticationService = new AuthenticationService(_dbContext, mockAuthenticationServiceLogger.Object, applicationSecurity);
        var mockHttpRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHeaders.Setup(o => o.Authorization).Returns($"Bearer {idToken}");
        mockHttpRequest.Setup(o => o.Headers).Returns(mockHeaders.Object);
        mockHttpContext.Setup(o => o.Request).Returns(mockHttpRequest.Object);
        mockHttpContext.Setup(o => o.Items).Returns(new Dictionary<object, object?>());

        return (requestAuthenticationCandinate, authenticationService, mockHttpContext);
    }
}