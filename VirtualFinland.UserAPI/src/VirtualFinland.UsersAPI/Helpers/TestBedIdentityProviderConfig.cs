using System.Text.Json.Nodes;

namespace VirtualFinland.UserAPI.Helpers;

public class TestBedIdentityProviderConfig : IIdentityProviderConfig
{
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

    public TestBedIdentityProviderConfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async void LoadOpenIDConfigUrl()
    {
        var testBedConfigUrl = _configuration["Testbed:OpenIDConfigurationURL"];
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(testBedConfigUrl);

        if (httpResponse.IsSuccessStatusCode)
        {
            var jsonData = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync());
            _issuer = jsonData?["issuer"]?.ToString();
            _jwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();
        }
    }
}