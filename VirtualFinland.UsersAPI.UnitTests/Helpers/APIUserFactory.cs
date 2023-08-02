using Bogus;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using Person = VirtualFinland.UserAPI.Models.UsersDatabase.Person;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class APIUserFactory
{
    /// <summary>
    ///     Create user with random guid
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static async Task<(Person user, ExternalIdentity externalIdentity, string identityId)> CreateAndGetLogInUser(
        UsersDbContext dbContext)
    {
        return await CreateAndGetLogInUser(dbContext, null);
    }

    /// <summary>
    ///     Create user with specified guid as Id
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static async Task<(Person user, ExternalIdentity externalIdentity, string identityId)> CreateAndGetLogInUser(
        UsersDbContext dbContext, Guid? userId)
    {
        var personDataAccessKey = dbContext.Cryptor.IdentityHelpers.CreateNewPersonDataAccessKey();
        dbContext.Cryptor.State.StartPersonDataQuery(personDataAccessKey);

        var dbUser = userId == null
            ? dbContext.Persons.Add(new PersonBuilder().WithPersonDataAccessKey(personDataAccessKey).Build())
            : dbContext.Persons.Add(new PersonBuilder().WithPersonDataAccessKey(personDataAccessKey).WithId(userId.Value).Build());

        var faker = new Faker();
        var issuer = faker.Random.String(10);
        var identityId = faker.Random.Guid().ToString();

        var identityHash = dbContext.Cryptor.SecretHash(identityId);
        var encryptedAccessKey = dbContext.Cryptor.Encrypt(personDataAccessKey, $"{dbUser.Entity.Id}::{issuer}::{identityId}");
        var externalIdentityPersonDataAccessKey = dbContext.Cryptor.Encrypt(encryptedAccessKey, identityId);

        dbContext.Cryptor.State.StartQuery("ExternalIdentity", identityId);
        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = issuer,
            KeyToPersonDataAccessKey = externalIdentityPersonDataAccessKey,
            IdentityHash = identityHash,
            UserId = dbUser.Entity.Id
        });

        await dbContext.SaveChangesAsync();

        return (dbUser.Entity, externalIdentity.Entity, identityId);
    }
}
