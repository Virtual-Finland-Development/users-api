using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data;
using FluentAssertions;
using VirtualFinland.UserAPI.Models;
using VirtualFinland.UsersAPI.UnitTests.Helpers;


namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User;

public class UserTests : APITestBase
{
    [Fact]
    public async void Should_GetUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        
        var query = new GetUser.Query(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);

        var handler = new GetUser.Handler(_dbContext);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Match<GetUser.User>(o => o.Id == dbEntities.user.Id && o.address == dbEntities.user.Address && o.FirstName == dbEntities.user.FirstName && o.LastName == dbEntities.user.LastName);
        
    }
    
    [Fact]
    public async void Should_UpdateUserAsync()
    {
        // Arrange
        var dbEntities = await APIUserFactory.CreateAndGetLogInUser(_dbContext);
        
        var command = new UpdateUser.UpdateUserCommand("New FirstName", "New LastName", null, null);
        command.SetAuth(dbEntities.externalIdentity.IdentityId, dbEntities.externalIdentity.Issuer);

        var handler = new UpdateUser.Handler(_dbContext);
        // Act

        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Match<UpdateUser.User>(o => o.Id == dbEntities.user.Id && o.FirstName == command.FirstName && o.LastName == command.LastName);
        
    }
}