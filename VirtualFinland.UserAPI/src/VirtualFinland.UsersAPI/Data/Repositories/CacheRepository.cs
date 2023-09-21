using System.Text.Json;
using StackExchange.Redis;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class CacheRepository : ICacheRepository
{
    private readonly IDatabase _database;

    public CacheRepository(IDatabase database)
    {
        _database = database;
    }

    public async Task<T> Get<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (value.HasValue)
        {
            return JsonSerializer.Deserialize<T>(value.ToString()) ?? throw new Exception($"Key {key} does not exist");
        }
        throw new Exception($"Key {key} does not exist");
    }

    public async Task Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        await _database.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);
    }

    public async Task Remove(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> Exists(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task Clear()
    {
        var endpoints = _database.Multiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _database.Multiplexer.GetServer(endpoint);
            await server.FlushDatabaseAsync();
        }
    }
}