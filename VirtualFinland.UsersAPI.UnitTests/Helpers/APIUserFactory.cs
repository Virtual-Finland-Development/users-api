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
    public static async Task<(Person user, ExternalIdentity externalIdentity)> CreateAndGetLogInUser(
        UsersDbContext dbContext)
    {
        var dbUser = dbContext.Persons.Add(new PersonBuilder().Build());

        var faker = new Faker();
        var identityId = faker.Random.Guid().ToString();
        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = faker.Random.String(10),
            IdentityId = identityId,
            IdentityHash = dbContext.Cryptor.Hash(identityId),
            UserId = dbUser.Entity.Id
        });

        await dbContext.SaveChangesAsync();

        return (dbUser.Entity, externalIdentity.Entity);
    }

    /// <summary>
    ///     Create user with specified guid as Id
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static async Task<(Person user, ExternalIdentity externalIdentity)> CreateAndGetLogInUser(
        UsersDbContext dbContext, Guid userId)
    {
        var dbUser = dbContext.Persons.Add(new PersonBuilder().WithId(userId).Build());

        var faker = new Faker();
        var identityId = faker.Random.Guid().ToString();
        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = faker.Random.String(10),
            IdentityId = identityId,
            IdentityHash = dbContext.Cryptor.Hash(identityId),
            UserId = dbUser.Entity.Id
        });

        await dbContext.SaveChangesAsync();

        return (dbUser.Entity, externalIdentity.Entity);
    }
}
