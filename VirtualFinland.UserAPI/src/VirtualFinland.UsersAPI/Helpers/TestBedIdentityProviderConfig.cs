using System.Text.Json.Nodes;

namespace VirtualFinland.UserAPI.Helpers;

public class TestBedIdentityProviderConfig : IIdentityProviderConfig
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private string? _issuer;
    private string? _jwksOptionsUrl;

    public string? JwksOptionsUrl
    {
        get { return _jwksOptionsUrl; }
    }

    public string? Issuer
    {
        get { return _issuer; }
    }

    public TestBedIdentityProviderConfig(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async void LoadOpenIDConfigUrl()
    {
        var testBedConfigUrl = _configuration["Testbed:OpenIDConfigurationURL"];
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(testBedConfigUrl);

        if (httpResponse.IsSuccessStatusCode)
        {
            var jsonData = JsonObject.Parse(await httpResponse.Content.ReadAsStringAsync());
            _issuer = jsonData?["issuer"]?.ToString();
            _jwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();
        }
    }
}