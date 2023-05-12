using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

// DynamoDb Cache 
public class AWSDynamoDBCacheManager
{
    protected readonly IAmazonDynamoDB _dynamoDbClient;
    protected readonly string _tableName;
    protected readonly TimeSpan _defaultExpiry = TimeSpan.FromDays(1);

    public AWSDynamoDBCacheManager(string tableName, TimeSpan? defaultExpiry = null, IAmazonDynamoDB? dynamoDbClient = null)
    {
        _tableName = tableName;
        _defaultExpiry = defaultExpiry ?? _defaultExpiry;
        _dynamoDbClient = dynamoDbClient ?? new AmazonDynamoDBClient();
    }

    public async Task<string?> GetAsync(string key)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "Key", new AttributeValue { S = key } }
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(request);

        if (response.Item == null)
        {
            return null;
        }

        return response.Item["Value"].S;
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (expiry == null)
        {
            expiry = _defaultExpiry;
        }

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "Key", new AttributeValue { S = key } },
                { "Value", new AttributeValue { S = value } }
            }
        };

        if (expiry != null)
        {
            request.Item.Add("Expiry", new AttributeValue { N = DateTimeOffset.UtcNow.Add(expiry.Value).ToUnixTimeSeconds().ToString() });
        }

        await _dynamoDbClient.PutItemAsync(request);
    }

    public async Task RemoveAsync(string key)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "Key", new AttributeValue { S = key } }
            }
        };

        await _dynamoDbClient.DeleteItemAsync(request);
    }
}