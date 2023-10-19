using Bogus;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Security.Models;
using Person = VirtualFinland.UserAPI.Models.UsersDatabase.Person;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class APIUserFactory
{
    public static string Issuer = "test-issuer";
    public static string Audience = "test-audience";

    /// <summary>
    ///     Create user with random guid
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static async Task<(Person user, ExternalIdentity externalIdentity, RequestAuthenticatedUser requestAuthenticatedUser)> CreateAndGetLogInUser(
        UsersDbContext dbContext)
    {
        var dbUser = dbContext.Persons.Add(new PersonBuilder().Build());

        var faker = new Faker();
        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = Issuer,
            IdentityId = faker.Random.Guid().ToString(),
            UserId = dbUser.Entity.Id
        });

        var requestAuthenticatedUser = new RequestAuthenticatedUser()
        {
            PersonId = dbUser.Entity.Id,
            IdentityId = externalIdentity.Entity.IdentityId ?? throw new Exception("IdentityId is null"),
            Issuer = externalIdentity.Entity.Issuer ?? throw new Exception("Issuer is null"),
            Audience = Audience,
            TraceId = Guid.NewGuid().ToString(),
        };

        await dbContext.SaveChangesAsync(requestAuthenticatedUser);

        return (dbUser.Entity, externalIdentity.Entity, requestAuthenticatedUser);
    }

    /// <summary>
    ///     Create user with specified guid as Id
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static async Task<(Person user, ExternalIdentity externalIdentity, RequestAuthenticatedUser requestAuthenticatedUser)> CreateAndGetLogInUser(
        UsersDbContext dbContext, Guid userId)
    {
        var dbUser = dbContext.Persons.Add(new PersonBuilder().WithId(userId).Build());

        var faker = new Faker();
        var externalIdentity = dbContext.ExternalIdentities.Add(new ExternalIdentity
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Issuer = faker.Random.String(10),
            IdentityId = faker.Random.Guid().ToString(),
            UserId = dbUser.Entity.Id
        });

        await dbContext.SaveChangesAsync();

        var requestAuthenticatedUser = new RequestAuthenticatedUser()
        {
            PersonId = dbUser.Entity.Id,
            IdentityId = externalIdentity.Entity.IdentityId ?? throw new Exception("IdentityId is null"),
            Issuer = externalIdentity.Entity.Issuer ?? throw new Exception("Issuer is null"),
            Audience = "test-audience",
            TraceId = Guid.NewGuid().ToString(),
        };

        return (dbUser.Entity, externalIdentity.Entity, requestAuthenticatedUser);
    }
}
