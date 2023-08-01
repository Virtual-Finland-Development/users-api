using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UsersAPI.UnitTests.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Identity;

public class IdentityTests : APITestBase
{
    [Fact]
    public async void Should_VerifyExistingLoginUser()
    {
        // Arrange
        var (user, externalIdentity, identityId) = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        var mockLogger = new Mock<ILogger<VerifyIdentityUser.Handler>>();
        var query = new VerifyIdentityUser.Query(identityId, externalIdentity.Issuer);
        var personsRepository = new PersonsRepository(_dbContext);
        var handler = new VerifyIdentityUser.Handler(personsRepository, mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Match<VerifyIdentityUser.User>(o => o.Id == user.Id && o.Created == user.Created && o.Modified == user.Modified);
    }

    [Fact]
    public async void Should_VerifyNewLoginUser()
    {
        // Arrange
        var faker = new Faker("en");
        var query = new VerifyIdentityUser.Query(faker.Random.Guid().ToString(), faker.Random.String(10));
        var mockLogger = new Mock<ILogger<VerifyIdentityUser.Handler>>();
        var personsRepository = new PersonsRepository(_dbContext);
        var handler = new VerifyIdentityUser.Handler(personsRepository, mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }
}
