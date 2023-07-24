using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

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

        return new UsersDbContext(options, new DatabaseEncryptionSecrets("12345678901234567890123456789012", "1234567890123456"), true);
    }
}