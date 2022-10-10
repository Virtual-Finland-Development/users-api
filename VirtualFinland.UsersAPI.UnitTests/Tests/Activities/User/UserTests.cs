using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data;
using FluentAssertions;
using VirtualFinland.UserAPI.Models;


namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User;

public class UserTests
{
    [Fact]
    public async void Should_GetUserAsync()
    {
        // Arrange
        var dbContext = GetMemoryContext();

        var dbUser = dbContext.Users.Add(new UserAPI.Models.User()
        {
            Address = "Address",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            FirstName = "FirstName",
            LastName = "LastName"
        });

        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity()
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = "issuer",
            IdentityId = "userid",
            UserId = dbUser.Entity.Id
        });

        await dbContext.SaveChangesAsync();

        var query = new GetUser.Query(externalIdentity.Entity.IdentityId, externalIdentity.Entity.Issuer);

        var handler = new GetUser.Handler(dbContext);
        // Act

        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(dbUser.Entity.Id);

    }
    
    public UsersDbContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
            .Options;
        
        return new UsersDbContext(options, true);
    }
}