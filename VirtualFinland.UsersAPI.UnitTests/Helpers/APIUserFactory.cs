using Bogus;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class APIUserFactory
{
    public static async Task<(User user, ExternalIdentity externalIdentity)> CreateAndGetLogInUser(
        UsersDbContext dbContext)
    {
        var faker = new Faker();

        var dbUser = dbContext.Users.Add(new User
        {
            StreetAddress = faker.Address.StreetAddress(),
            ZipCode = faker.Address.ZipCode(),
            City = faker.Address.City(),
            Country = faker.Address.Country(),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            FirstName = faker.Person.FirstName,
            LastName = faker.Person.LastName,
            JobsDataConsent = true,
            ImmigrationDataConsent = false,
            CitizenshipCode = "FR",
            CountryOfBirthCode = "FR",
            OccupationCode = "4012",
            NativeLanguageCode = "FR",
            Gender = Gender.Male,
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now)
        });

        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity
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
