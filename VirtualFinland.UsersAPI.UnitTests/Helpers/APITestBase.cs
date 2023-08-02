using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;

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

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var auditInterceptor = new AuditInterceptor(loggerFactory.CreateLogger<IAuditInterceptor>());
        return new UsersDbContext(options, new DatabaseEncryptionSettings("12345678901234567890123456789012", "1234567890123456"), auditInterceptor, true);
    }
}