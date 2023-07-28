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

    public async Task<Person> GetOrCreatePerson(string issuer, string identityId, CancellationToken cancellationToken)
    {
        var identityHash = _usersDbContext.Cryptor.Hash(identityId);

        _usersDbContext.Cryptor.State.StartQuery("ExternalIdentity", identityHash);
        var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(
            o => o.IdentityHash == identityHash && o.Issuer == issuer, cancellationToken);

        // Create a new system user if no one found based on given authentication information
        if (externalIdentity is null)
        {
            _usersDbContext.Cryptor.State.ClearQuery("Person");
            var newDbUSer = await _usersDbContext.Persons.AddAsync(
                new Person { Created = DateTime.UtcNow, Modified = DateTime.UtcNow }, cancellationToken);

            var newExternalIdentity = await _usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity
            {
                Issuer = issuer,
                IdentityId = identityId,
                IdentityHash = identityHash,
                UserId = newDbUSer.Entity.Id,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }, cancellationToken);

            await _usersDbContext.SaveChangesAsync(cancellationToken);
            newDbUSer.Entity.EncryptionKey = identityId; //@TODO Use identity access key instead
            return newDbUSer.Entity;
        }

        _usersDbContext.Cryptor.State.StartQuery("Person", externalIdentity.IdentityId); //@TODO Use identity access key instead
        var dbUser =
            await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

        return dbUser;
    }
}