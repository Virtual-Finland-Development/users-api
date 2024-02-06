namespace VirtualFinland.UserAPI.Data.Repositories;

public interface IPersonRepository
{
    /// <summary>
    /// Deletes a person and relations from the database
    /// </summary>
    Task DeletePerson(Guid personId, CancellationToken cancellationToken);

}