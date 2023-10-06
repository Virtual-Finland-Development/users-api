using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UsersAPI.UnitTests.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Identity;

public class IdentityTests : APITestBase
{
    [Fact]
    public async void Should_VerifyExistingLoginUser()
    {
        // Arrange
        var (user, externalIdentity, requestAuthenticatedUser) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var (requestAuthenticationCandinate, authenticationService, mockHttpContext) = GetGoodLoginRequestSituation(requestAuthenticatedUser);

        var query = new VerifyIdentityPerson.Query(mockHttpContext.Object);
        var handler = new VerifyIdentityPerson.Handler(authenticationService);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Match<VerifyIdentityPerson.User>(o => o.Id == requestAuthenticatedUser.PersonId);
    }

    [Fact]
    public async void Should_VerifyNewLoginUser()
    {
        // Arrange
        var faker = new Faker("en");
        var requestAuthenticatedUser = new RequestAuthenticationCandinate()
        {
            IdentityId = faker.Random.Guid().ToString(),
            Issuer = APIUserFactory.Issuer,
            Audience = APIUserFactory.Audience,
        };

        var (_, authenticationService, mockHttpContext) = GetGoodLoginRequestSituation(requestAuthenticatedUser);

        var query = new VerifyIdentityPerson.Query(mockHttpContext.Object);
        var handler = new VerifyIdentityPerson.Handler(authenticationService);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }
}
