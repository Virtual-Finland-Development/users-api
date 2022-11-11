using Microsoft.EntityFrameworkCore.ChangeTracking;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UsersAPI.UnitTests.Helpers;

namespace VirtualFinland.UsersAPI.UnitTests.Stubs;

public class StubUserRepository : IUserRepository
{

    public Task<User> GetUser(Guid? id, CancellationToken cancellationToken)
    {
        var dbEntities = ApiUserFactory.CreateAndGetLogInUser();
        return Task.FromResult(dbEntities.user);
    }
    public Task<SearchProfile?> GetUserDefaultSearchProfile(Guid? userId, bool isDefault, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SearchProfile()
        {
            Id = Guid.NewGuid(),
            Name = "SearchProfile"
        });
    }
    public IQueryable<SearchProfile> GetUserSearchProfiles(Guid? userId)
    {
        throw new NotImplementedException();
    }
    public Task<SearchProfile> GetSearchProfile(Guid? id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    public Task UpdateUser(User user, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public ValueTask<EntityEntry<SearchProfile>> AddSearchProfile(SearchProfile searchProfile, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    public Task UpdateSearchProfile(SearchProfile searchProfile, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}