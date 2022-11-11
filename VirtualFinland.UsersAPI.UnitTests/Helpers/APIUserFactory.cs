using Bogus;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public static class ApiUserFactory
{
    public static (User user, ExternalIdentity externalIdentity) CreateAndGetLogInUser()
    {
        var faker = new Faker("en");
        
        var dbUser = new User()
        {
            Address = faker.Address.FullAddress(),
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
            Gender = "1",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now)
        };

        var externalIdentity = new ExternalIdentity()
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = faker.Random.String(10),
            IdentityId = faker.Random.Guid().ToString(),
            UserId = dbUser.Id
        };

        return (dbUser, externalIdentity);
    }
}