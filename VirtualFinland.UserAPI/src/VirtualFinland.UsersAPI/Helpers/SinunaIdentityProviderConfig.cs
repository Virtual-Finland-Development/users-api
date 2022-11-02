using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace VirtualFinland.UserAPI.Helpers;

public class SinunaIdentityProviderConfig : IIdentityProviderConfig
{

    private readonly IConfiguration _configuration;
    private string? _issuer;
    private string? _jwksOptionsUrl;
    private readonly int _configUrlMaxRetryCount = 5;
    /// <summary>
    /// How long to wait in milliseconds before trying again to retrieve the openid configs
    /// </summary>
    private readonly int _configUrlRetryWaitTime = 3000;

    public string? JwksOptionsUrl
    {
        get { return _jwksOptionsUrl; }
    }

    public string? Issuer
    {
        get { return _issuer; }
    }

    public SinunaIdentityProviderConfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async void LoadOpenIdConfigUrl()
    {
        var configUrl = _configuration["Sinuna:OpenIDConfigurationURL"];
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:106.0) Gecko/20100101 Firefox/106.0");
        var httpResponse = await httpClient.GetAsync(configUrl);
        
        for (int retryCount = 0; retryCount < _configUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonData = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync());
                _issuer = jsonData?["issuer"]?.ToString();
                _jwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();

                if (!string.IsNullOrEmpty(_issuer) && !string.IsNullOrEmpty(_jwksOptionsUrl))
                {
                    break;    
                }
            }    
            Thread.Sleep(_configUrlRetryWaitTime);
        }
        
        // If all retries fail, then send an exception since the security information is critical to the functionality of the backend
        if (string.IsNullOrEmpty(_issuer) || string.IsNullOrEmpty(_jwksOptionsUrl))
        {
            throw new ApplicationException("Failed to retrieve TestBed OpenID configurations.");
        }
    }
}