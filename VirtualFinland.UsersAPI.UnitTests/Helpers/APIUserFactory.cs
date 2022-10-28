using Bogus;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class APIUserFactory
{
    public async static Task<(User user, ExternalIdentity externalIdentity)> CreateAndGetLogInUser(UsersDbContext dbContext)
    {
        var faker = new Faker("en");
        
        var dbUser = dbContext.Users.Add(new User()
        {
            Address = faker.Address.FullAddress(),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            FirstName = faker.Person.FirstName,
            LastName = faker.Person.LastName,
            JobsDataConsent = true,
            ImmigrationDataConsent = false,
            NationalityISOCode = "FR",
            CountryOfBirthISOCode = "FR",
            OccupationISCOCode = "4012",
            NativeLanguageISOCode = "FR",
            Gender = "1",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now)
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