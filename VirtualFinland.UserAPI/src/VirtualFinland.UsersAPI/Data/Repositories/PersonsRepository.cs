using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class PersonsRepository : IPersonsRepository
{
    private readonly IServiceProvider _services;

    public PersonsRepository(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<Person> GetPerson(string issuer, string identityId, CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var identityHash = usersDbContext.Cryptor.SecretHash(identityId);
        usersDbContext.Cryptor.State.StartQuery("ExternalIdentity", identityId);
        var externalIdentity = await usersDbContext.ExternalIdentities.SingleOrDefaultAsync(o => o.IdentityHash == identityHash && o.Issuer == issuer, cancellationToken) ?? throw new InvalidOperationException("Person not found");

        var accessKeyForPersonData = usersDbContext.Cryptor.IdentityHelpers.DecryptExternalIdentityAccessKeyForPersonData(externalIdentity, identityId);

        usersDbContext.Cryptor.State.StartQuery("Person", accessKeyForPersonData);
        return await usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

    }

    public async Task<Person> GetOrCreatePerson(string issuer, string identityId, CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var identityHash = usersDbContext.Cryptor.SecretHash(identityId);
        usersDbContext.Cryptor.State.StartQuery("ExternalIdentity", identityId);
        var externalIdentity = await usersDbContext.ExternalIdentities.SingleOrDefaultAsync(
            o => o.IdentityHash == identityHash && o.Issuer == issuer, cancellationToken);

        // Create a new system user if no one found based on given authentication information
        if (externalIdentity is null)
        {
            var createdAt = DateTime.UtcNow;
            var personDataAccessKey = usersDbContext.Cryptor.IdentityHelpers.CreateNewPersonDataAccessKey();

            usersDbContext.Cryptor.State.StartQuery("Person", personDataAccessKey);
            var newDbUSer = await usersDbContext.Persons.AddAsync(
                new Person
                {
                    Created = createdAt,
                    Modified = createdAt,
                    PersonDataAccessKey = personDataAccessKey
                }, cancellationToken);

            var newAccessKeyForPersonData = usersDbContext.Cryptor.IdentityHelpers.EncryptExternalIdentityAccessKeyForPersonData(newDbUSer.Entity.PersonDataAccessKey, newDbUSer.Entity.Id, issuer, identityId);

            var newExternalIdentity = await usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity
            {
                Issuer = issuer,
                KeyToPersonDataAccessKey = newAccessKeyForPersonData,
                IdentityHash = identityHash,
                UserId = newDbUSer.Entity.Id,
                Created = createdAt,
                Modified = createdAt,
            }, cancellationToken);

            await usersDbContext.SaveChangesAsync(cancellationToken);
            return newDbUSer.Entity;
        }

        // Decrypt person data access key
        var accessKeyForPersonData = usersDbContext.Cryptor.IdentityHelpers.DecryptExternalIdentityAccessKeyForPersonData(externalIdentity, identityId);
        usersDbContext.Cryptor.State.StartQuery("Person", accessKeyForPersonData);
        var dbUser =
            await usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

        return dbUser;
    }
}