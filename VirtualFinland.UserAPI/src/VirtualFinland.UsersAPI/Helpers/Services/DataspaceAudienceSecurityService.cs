using System.Net;
using System.Text.Json;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Configurations;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class DataspaceAudienceSecurityService
{
    private readonly AudienceGuardServiceConfig _config;
    private readonly ICacheRepository _cacheRepository;

    public DataspaceAudienceSecurityService(AudienceGuardServiceConfig config, ICacheRepository cacheRepository)
    {
        _config = config;
        _cacheRepository = cacheRepository;
    }


    public async Task VerifyAudience(string audience)
    {
        if (!_config.IsEnabled) return;

        // If audience is cached, return
        if (await _cacheRepository.Exists(audience)) return;

        try
        {
            var verifyUrl = $"{_config.ApiEndpoint}/external-apps/{audience}/public";

            var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(verifyUrl);
            response.EnsureSuccessStatusCode();

            var responseData = await JsonSerializer.DeserializeAsync<AudienceVerifyResponse>(await response.Content.ReadAsStreamAsync()) ??
                   throw new NotAuthorizedException("Could not verify audience");

            if (!_config.AllowedGroups.Contains(responseData.Group)) throw new NotAuthorizedException("Audience group is not allowed");

            // Cache audience for 24 hours
            await _cacheRepository.Set(audience, responseData, TimeSpan.FromHours(24));
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound) { throw new NotAuthorizedException("Could not resolve audience"); }
            throw new NotAuthorizedException(e.Message);
        }
    }

    private record AudienceVerifyResponse
    {
        public string Name { get; init; } = default!;
        public string Group { get; init; } = default!;
        public string? AppUrl { get; init; } = default!;
        public string? PrivacyPolicy { get; init; } = default!;
        public string? PartyConfigurationDomain { get; init; } = default!;
        public string? GdprContact { get; init; } = default!;
    }
}
