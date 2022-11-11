using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _usersDbContext;

    public UserRepository(UsersDbContext usersDbContext)
    {
        _usersDbContext = usersDbContext;
    }
    public Task<User> GetUser(Guid? id, CancellationToken cancellationToken)
    {
        return _usersDbContext.Users.SingleAsync(o => o.Id == id, cancellationToken);
    }
    public Task<SearchProfile?> GetUserDefaultSearchProfile(Guid? userId, bool isDefault, CancellationToken cancellationToken)
    {
        return _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == isDefault && o.UserId == userId, cancellationToken);
    }
    public IQueryable<SearchProfile> GetUserSearchProfiles(Guid? userId)
    {
        return _usersDbContext.SearchProfiles.Where(o => o.UserId == userId);
    }
    public Task<SearchProfile> GetSearchProfile(Guid? id, CancellationToken cancellationToken)
    {
        return _usersDbContext.SearchProfiles.SingleAsync(o => o.Id == id, cancellationToken);
    }
    public async Task UpdateUser(User user, CancellationToken cancellationToken)
    {
        _usersDbContext.Users.Update(user);
        await _usersDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
    }
    public ValueTask<EntityEntry<SearchProfile>> AddSearchProfile(SearchProfile searchProfile, CancellationToken cancellationToken)
    {
        return _usersDbContext.SearchProfiles.AddAsync(searchProfile, cancellationToken);
    }
    public async Task UpdateSearchProfile(SearchProfile searchProfile, CancellationToken cancellationToken)
    {
        _usersDbContext.SearchProfiles.Update(searchProfile);
        await _usersDbContext.SaveChangesAsync(cancellationToken);
    }
}