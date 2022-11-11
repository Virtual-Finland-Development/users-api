using Microsoft.EntityFrameworkCore.ChangeTracking;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public interface IUserRepository
{
    Task<User> GetUser(Guid? id, CancellationToken cancellationToken);
    Task<SearchProfile?> GetUserDefaultSearchProfile(Guid? userId, bool isDefault, CancellationToken cancellationToken);
    
    IQueryable<SearchProfile> GetUserSearchProfiles(Guid? userId);

    
    Task<SearchProfile> GetSearchProfile(Guid? id, CancellationToken cancellationToken);


    Task UpdateUser(User user, CancellationToken cancellationToken);

    ValueTask<EntityEntry<SearchProfile>> AddSearchProfile(SearchProfile searchProfile, CancellationToken cancellationToken);

    Task UpdateSearchProfile(SearchProfile searchProfile, CancellationToken cancellationToken);
}