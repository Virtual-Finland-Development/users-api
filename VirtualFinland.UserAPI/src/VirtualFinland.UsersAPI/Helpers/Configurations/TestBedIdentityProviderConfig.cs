using System.Text.Json.Nodes;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class TestBedIdentityProviderConfig : IIdentityProviderConfig
{
    private readonly IConfiguration _configuration;
    private readonly AWSDynamoDBJsonObjectCacheManager _awsDynamoDBCache;
    private readonly string _cacheName;

    private string? _issuer;
    private string? _jwksOptionsUrl;
    private const int _configUrlMaxRetryCount = 5;

    /// <summary>
    /// How long to wait in milliseconds before trying again to retrieve the openid configs
    /// </summary>
    private const int _configUrlRetryWaitTime = 3000;

    public string? JwksOptionsUrl
    {
        get { return _jwksOptionsUrl; }
    }

    public string? Issuer
    {
        get { return _issuer; }
    }

    public TestBedIdentityProviderConfig(IConfiguration configuration, AWSDynamoDBJsonObjectCacheManager awsDynamoDBCache)
    {
        _configuration = configuration;
        _awsDynamoDBCache = awsDynamoDBCache;
        _cacheName = "TestbedOpenIdConfig";
    }

    public async Task LoadOpenIdConfig()
    {
        if (_jwksOptionsUrl != null && _issuer != null)
        {
            return;
        }

        var cachedConfig = await _awsDynamoDBCache.GetAsync<IIdentityProviderConfig>(_cacheName);
        if (cachedConfig != null)
        {
            _issuer = cachedConfig.Issuer;
            _jwksOptionsUrl = cachedConfig.JwksOptionsUrl;
            return;
        }

        var (issuer, jwksOptionsUrl) = await RetrieveOpenIdConfig();
        _issuer = issuer;
        _jwksOptionsUrl = jwksOptionsUrl;

        await _awsDynamoDBCache.SetAsync(_cacheName, this);
    }

    private async Task<(string, string)> RetrieveOpenIdConfig()
    {
        var testBedConfigUrl = _configuration["Testbed:OpenIDConfigurationURL"];
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(testBedConfigUrl);

        var issuer = string.Empty;
        var jwksOptionsUrl = string.Empty;

        for (int retryCount = 0; retryCount < _configUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonData = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync());
                issuer = jsonData?["issuer"]?.ToString();
                jwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();

                if (!string.IsNullOrEmpty(issuer) && !string.IsNullOrEmpty(jwksOptionsUrl))
                {
                    break;
                }
            }
            await Task.Delay(_configUrlRetryWaitTime);
        }

        // If all retries fail, then send an exception since the security information is critical to the functionality of the backend
        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(jwksOptionsUrl))
        {
            throw new Exception("Failed to retrieve TestBed OpenID configurations.");
        }

        return (issuer, jwksOptionsUrl);
    }
}