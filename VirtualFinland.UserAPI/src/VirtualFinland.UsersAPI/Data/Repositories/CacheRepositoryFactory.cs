using StackExchange.Redis;

namespace VirtualFinland.UserAPI.Data.Repositories;

class CacheRepositoryFactory : ICacheRepositoryFactory
{
    private readonly IDatabase _database;
    private readonly string _factoryKeyPrefix;

    public CacheRepositoryFactory(IDatabase database, string factoryKeyPrefix = "")
    {
        _database = database;
        _factoryKeyPrefix = factoryKeyPrefix;
        if (!string.IsNullOrEmpty(_factoryKeyPrefix)) _factoryKeyPrefix += ":";
    }

    public ICacheRepository Create(string keyPrefix = "")
    {

        return new CacheRepository(_database, $"{_factoryKeyPrefix}{keyPrefix}");
    }
}