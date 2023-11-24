using StackExchange.Redis;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class CacheRepositoryFactory : ICacheRepositoryFactory
{
    private readonly IDatabase _database;
    private readonly string _factoryKeyPrefix;
    static readonly List<string> KnownKeys = new(); // Static list of all known cache repository keys

    public CacheRepositoryFactory(IDatabase database, string factoryKeyPrefix = "")
    {
        _database = database;
        _factoryKeyPrefix = factoryKeyPrefix;
        if (!string.IsNullOrEmpty(_factoryKeyPrefix)) _factoryKeyPrefix = $"{_factoryKeyPrefix}:";
    }

    public ICacheRepository Create(string keyPrefix = "")
    {
        var cacheRepositoryKeyPrefix = $"{_factoryKeyPrefix}{keyPrefix}";

        // Safety check to avoid accidentally overwriting existing cache repositories
        if (KnownKeys.Contains(cacheRepositoryKeyPrefix)) throw new ArgumentException($"Key prefix {cacheRepositoryKeyPrefix} already in use");
        KnownKeys.Add(cacheRepositoryKeyPrefix);

        return new CacheRepository(_database, cacheRepositoryKeyPrefix);
    }
}