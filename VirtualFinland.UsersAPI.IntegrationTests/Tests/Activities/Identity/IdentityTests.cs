using Bogus;
using FluentAssertions;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UsersAPI.UnitTests.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Identity;

public class IdentityTests : APITestBase
{
    [Fact]
    public async void Should_VerifyExistingLoginUser()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);

        var query = new GetTestbedIdentityUser.Query(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);
        
        var handler = new GetTestbedIdentityUser.GetTestbedIdentityUserHandler(_dbContext);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Match<GetTestbedIdentityUser.User>(o => o.Id == dbEntities.user.Id && o.Created == dbEntities.user.Created && o.Modified == dbEntities.user.Modified);
    }
    
    [Fact]
    public async void Should_VerifyNewLoginUser()
    {
        var faker = new Faker("en");
        // Arrange
        var query = new GetTestbedIdentityUser.Query(faker.Random.Guid().ToString(), faker.Random.String(10));
        
        var handler = new GetTestbedIdentityUser.GetTestbedIdentityUserHandler(_dbContext);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }
}