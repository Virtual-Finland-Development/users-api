using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public interface IPersonsRepository
{
    Task<Person> GetPerson(string issuer, string identityId, CancellationToken cancellationToken);
    Task<Person> GetOrCreatePerson(string issuer, string identityId, CancellationToken cancellationToken);
}