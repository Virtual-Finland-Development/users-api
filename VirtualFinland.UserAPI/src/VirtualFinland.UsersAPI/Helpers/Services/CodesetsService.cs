using System.Text.Json;
using VirtualFinland.UserAPI.Helpers.Configurations;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class CodesetsService
{
    private readonly CodesetConfig _config;
    private readonly HttpClient _httpClient;

    public CodesetsService(CodesetConfig codesetConfig)
    {
        _config = codesetConfig;
        _httpClient = new HttpClient(
            new HttpRequestTimeoutHandler
            {
                DefaultTimeout = TimeSpan.FromSeconds(9),
                DefaultTimeoutMessage = "Codesets request timeout",
                InnerHandler = new HttpClientHandler()
            }
        );
    }

    public async Task<T> GetResource<T>(string? resourceName = null)
    {
        var resourceIdent = resourceName ?? typeof(T).Name.ToLowerInvariant();
        var resourceEndpoint = _config.GetResourceEndpoint(resourceIdent);

        var httpResponseMessage = await _httpClient.GetAsync(resourceEndpoint);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get resource {resourceIdent} from {resourceEndpoint}");
        }

        return JsonSerializer.Deserialize<T>(await httpResponseMessage.Content.ReadAsStringAsync()) ?? throw new Exception($"Failed to deserialize resource {resourceName} from {resourceEndpoint}");
    }
}
