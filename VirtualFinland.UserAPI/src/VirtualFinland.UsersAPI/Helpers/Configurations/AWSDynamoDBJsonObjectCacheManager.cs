using System.Text.Json;
using Amazon.DynamoDBv2;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

// DynamoDb JSON Cache 
public class AWSDynamoDBJsonObjectCacheManager : AWSDynamoDBCacheManager
{
    public AWSDynamoDBJsonObjectCacheManager(string tableName, TimeSpan? defaultExpiry = null, IAmazonDynamoDB? dynamoDbClient = null) : base(tableName, defaultExpiry, dynamoDbClient)
    {
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await base.GetAsync(key);

        if (value == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await base.SetAsync(key, JsonSerializer.Serialize(value), expiry);
    }
}