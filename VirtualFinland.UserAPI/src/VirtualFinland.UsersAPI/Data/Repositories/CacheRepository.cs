using System.Text.Json;
using StackExchange.Redis;

namespace VirtualFinland.UserAPI.Data.Repositories;

public class CacheRepository : ICacheRepository
{
    private readonly IDatabase _database;
    private readonly string _keyPrefix;

    public CacheRepository(IDatabase database, string keyPrefix = "")
    {
        _database = database;
        _keyPrefix = keyPrefix;
        if (!string.IsNullOrEmpty(_keyPrefix))
        {
            _keyPrefix += ":";
        }
    }

    public async Task<T> Get<T>(string key)
    {
        var keyActual = ResolveKey(key);
        var value = await _database.StringGetAsync(keyActual);
        if (value.HasValue)
        {
            return JsonSerializer.Deserialize<T>(value.ToString()) ?? throw new Exception($"Key {keyActual} has invalid value");
        }
        throw new Exception($"Key {keyActual} does not exist");
    }

    public async Task Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        await _database.StringSetAsync(ResolveKey(key), JsonSerializer.Serialize(value), expiry);
    }

    public async Task Remove(string key)
    {
        await _database.KeyDeleteAsync(ResolveKey(key));
    }

    public async Task<bool> Exists(string key)
    {
        return await _database.KeyExistsAsync(ResolveKey(key));
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

    private string ResolveKey(string key)
    {
        return $"{_keyPrefix}{key}";
    }
}