using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class TestBedConsentProviderConfig : IConsentProviderConfig
{
    private readonly string _jwksJsonUrl;
    private readonly string _issuer;
    private readonly string _consentVerifyUrl;

    private List<JsonWebKey> _keys = new List<JsonWebKey>();

    private readonly int _configUrlMaxRetryCount = 5;
    /// <summary>
    /// How long to wait in milliseconds before trying again to retrieve the openid configs
    /// </summary>
    private readonly int _configUrlRetryWaitTime = 3000;

    public string Issuer
    {
        get { return _issuer; }
    }

    public JsonWebKey? GetKey(string kid)
    {
        return _keys.FirstOrDefault(k => k.Kid == kid);
    }

    public string ConsentVerifyUrl
    {
        get { return _consentVerifyUrl; }
    }

    public TestBedConsentProviderConfig(IConfiguration configuration)
    {
        _jwksJsonUrl = configuration["Testbed:ConsentJwksJsonUrl"];
        _issuer = configuration["Testbed:ConsentIssuer"];
        _consentVerifyUrl = configuration["Testbed:ConsentVerifyUrl"];
    }

    public async void LoadPublicKeys()
    {
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(_jwksJsonUrl);

        for (int retryCount = 0; retryCount < _configUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonData = JsonSerializer.Deserialize<JwksJsonResponse>(await httpResponse.Content.ReadAsStringAsync());
                if (jsonData == null || jsonData.Keys == null)
                {
                    throw new ApplicationException("Failed to retrieve TestBed Consent public key configurations");
                }

                _keys = jsonData.Keys.Select(k => new JsonWebKey()
                {
                    Kty = k.Kty,
                    Use = k.Use,
                    Kid = k.Kid,
                    N = k.N,
                    E = k.E
                }).ToList();
            }
            Thread.Sleep(_configUrlRetryWaitTime);
        }
    }

    class JwksJsonResponse
    {
        [JsonPropertyName("keys")]
        public List<Jwk>? Keys { get; set; }
    }

    class Jwk
    {
        [JsonPropertyName("kty")]
        public string? Kty { get; set; }
        [JsonPropertyName("use")]
        public string? Use { get; set; }
        [JsonPropertyName("kid")]
        public string? Kid { get; set; }
        [JsonPropertyName("n")]
        public string? N { get; set; }
        [JsonPropertyName("e")]
        public string? E { get; set; }
    }
}