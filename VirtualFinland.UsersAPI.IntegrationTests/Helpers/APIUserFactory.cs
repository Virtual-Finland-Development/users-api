using Bogus;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class APIUserFactory
{
    public async static Task<(User user, ExternalIdentity externalIdentity)> CreateAndGetLogInUser(UsersDbContext dbContext)
    {
        var faker = new Faker("en");
        
        var dbUser = dbContext.Users.Add(new UserAPI.Models.User()
        {
            Address = faker.Address.FullAddress(),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            FirstName = faker.Person.FirstName,
            LastName = faker.Person.LastName,
            JobsDataConsent = true,
            ImmigrationDataConsent = false
        });

        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity()
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = faker.Random.String(10),
            IdentityId = faker.Random.Guid().ToString(),
            UserId = dbUser.Entity.Id
        });

        await dbContext.SaveChangesAsync();

        return (dbUser.Entity, externalIdentity.Entity);
    }
}