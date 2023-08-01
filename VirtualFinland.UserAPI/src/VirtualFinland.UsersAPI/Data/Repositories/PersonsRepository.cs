using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class PersonsRepository : IPersonsRepository
{
    private readonly UsersDbContext _usersDbContext;

    public PersonsRepository(UsersDbContext context)
    {
        _usersDbContext = context;
    }

    public async Task<Person> GetPerson(string issuer, string identityId, CancellationToken cancellationToken)
    {
        var identityHash = _usersDbContext.Cryptor.SecretHash(identityId);
        _usersDbContext.Cryptor.State.StartQuery("ExternalIdentity", identityId);
        var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityHash == identityHash && o.Issuer == issuer, cancellationToken);

        var accessKeyForPersonData = _usersDbContext.Cryptor.IdentityHelpers.DecryptExternalIdentityAccessKeyForPersonData(externalIdentity, identityId);
        _usersDbContext.Cryptor.State.StartQuery("Person", accessKeyForPersonData);
        return await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
    }

    public async Task<Person> GetOrCreatePerson(string issuer, string identityId, CancellationToken cancellationToken)
    {
        var identityHash = _usersDbContext.Cryptor.SecretHash(identityId);
        _usersDbContext.Cryptor.State.StartQuery("ExternalIdentity", identityId);
        var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(
            o => o.IdentityHash == identityHash && o.Issuer == issuer, cancellationToken);

        // Create a new system user if no one found based on given authentication information
        if (externalIdentity is null)
        {
            var createdAt = DateTime.UtcNow;
            var personDataAccessKey = _usersDbContext.Cryptor.IdentityHelpers.CreateNewPersonDataAccessKey();

            _usersDbContext.Cryptor.State.StartQuery("Person", personDataAccessKey);
            var newDbUSer = await _usersDbContext.Persons.AddAsync(
                new Person
                {
                    Created = createdAt,
                    PersonDataAccessKey = personDataAccessKey
                }, cancellationToken);

            var newAccessKeyForPersonData = _usersDbContext.Cryptor.IdentityHelpers.EncryptExternalIdentityAccessKeyForPersonData(newDbUSer.Entity.PersonDataAccessKey, newDbUSer.Entity.Id, issuer, identityId);

            var newExternalIdentity = await _usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity
            {
                Issuer = issuer,
                PersonDataAccessKey = newAccessKeyForPersonData,
                IdentityHash = identityHash,
                UserId = newDbUSer.Entity.Id,
                Created = createdAt,
            }, cancellationToken);

            await _usersDbContext.SaveChangesAsync(cancellationToken);
            return newDbUSer.Entity;
        }

        // Decrypt person data access key
        var accessKeyForPersonData = _usersDbContext.Cryptor.IdentityHelpers.DecryptExternalIdentityAccessKeyForPersonData(externalIdentity, identityId);
        _usersDbContext.Cryptor.State.StartQuery("Person", accessKeyForPersonData);
        var dbUser =
            await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

        return dbUser;
    }
}