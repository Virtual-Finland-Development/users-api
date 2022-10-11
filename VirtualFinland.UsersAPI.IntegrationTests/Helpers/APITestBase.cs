using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class APITestBase
{
    protected readonly UsersDbContext _dbContext;

    public APITestBase()
    {
        _dbContext = GetMemoryContext();
    }

    private UsersDbContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
            .Options;
        
        return new UsersDbContext(options, true);
    }
}