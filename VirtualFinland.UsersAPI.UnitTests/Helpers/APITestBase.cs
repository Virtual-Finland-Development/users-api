using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    protected Mock<IServiceProvider> GetMockedServiceProvider()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(UsersDbContext)))
            .Returns(_dbContext);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);

        serviceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        return serviceProvider;
    }
}