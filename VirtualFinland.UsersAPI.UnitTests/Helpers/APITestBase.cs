using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
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
        var auditInterceptor = new AuditInterceptor(new Mock<ILogger<IAuditInterceptor>>().Object);

        return new UsersDbContext(options, auditInterceptor, true);
    }
}