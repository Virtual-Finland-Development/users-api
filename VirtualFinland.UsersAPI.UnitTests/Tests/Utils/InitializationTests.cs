using FluentAssertions;
using Moq;
using StackExchange.Redis;
using VirtualFinland.UserAPI.Data.Repositories;


namespace VirtualFinland.UsersAPI.UnitTests.Tests.Utils;

public class InitializationTests
{

    [Fact]
    /// <summary>
    /// 
    /// </summary>
    public void CreatingCacheRepositoryWithExistingDuplicatePrefixes_shouldThrowArgumentException()
    {
        // Arrange
        var cacheRepositoryFactory = new CacheRepositoryFactory(new Mock<IDatabase>().Object);
        cacheRepositoryFactory.Create("test1");
        cacheRepositoryFactory.Create("test2");
        cacheRepositoryFactory.Create("test3");

        // Act
        var act = () => cacheRepositoryFactory.Create("test2");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
