using Bogus;
using Microsoft.EntityFrameworkCore;
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
        var issuer = faker.Random.AlphaNumeric(10);
        var identityId = $"id_{faker.Random.Guid()}";

        var identityHash = dbContext.Cryptor.SecretHash(identityId);

        var keyToPersonDataAccessKey = dbContext.Cryptor.IdentityHelpers.EncryptExternalIdentityAccessKeyForPersonData(personDataAccessKey, dbUser.Entity.Id, issuer, identityId);
        dbContext.Cryptor.State.StartQuery("ExternalIdentity", identityId);
        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = issuer,
            KeyToPersonDataAccessKey = keyToPersonDataAccessKey,
            IdentityHash = identityHash,
            UserId = dbUser.Entity.Id
        });

        await dbContext.SaveChangesAsync();

        // Reload the models so the DecryptionInterceptor fires up
        externalIdentity.State = EntityState.Detached;
        var reloadedExternalIdentity = await dbContext.ExternalIdentities.FindAsync(externalIdentity.Entity.Id) ?? throw new Exception("External identity not found");
        dbUser.State = EntityState.Detached;
        var reloadedUser = await dbContext.Persons.FindAsync(dbUser.Entity.Id) ?? throw new Exception("User not found");

        return (reloadedUser, reloadedExternalIdentity, identityId);
    }
}
