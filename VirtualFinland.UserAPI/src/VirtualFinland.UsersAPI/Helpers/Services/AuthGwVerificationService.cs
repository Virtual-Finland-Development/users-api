using System.Net.Http.Headers;
using System.Text.Json;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models.Shared;

namespace VirtualFinland.UserAPI.Helpers.Services;


public class AuthGwVerificationService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthGwVerificationService> _logger;

    public AuthGwVerificationService(
        ILogger<AuthGwVerificationService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    ///     Verifies the token from AuthGW
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requireConsentTokenForDataSource">If provided, verify the consent for the data source</param>
    /// <param name="requireConsentUserId">If provided, verify the consent user id matches</param>
    public async Task<AuthorizeResponse> VerifyTokens(HttpRequest request, string requireConsentTokenForDataSource = "", string requireConsentUserId = "")
    {
        try
        {
            var token = request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
            if (string.IsNullOrEmpty(token))
                throw new NotAuthorizedException("Token is missing");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            httpClient.DefaultRequestHeaders.Add(
                Constants.Headers.XAuthorizationContext,
                Constants.Web.AuthGwApplicationContext);

            if (requireConsentTokenForDataSource.Length > 0)
            {
                if (!request.Headers.ContainsKey(Constants.Headers.XConsentToken))
                    throw new NotAuthorizedException("Consent token is missing");

                httpClient.DefaultRequestHeaders.Add(
                    Constants.Headers.XConsentToken,
                    request.Headers[Constants.Headers.XConsentToken].ToString());

                httpClient.DefaultRequestHeaders.Add(Constants.Headers.XconsentDataSource,
                    requireConsentTokenForDataSource);

                if (requireConsentUserId.Length > 0)
                    httpClient.DefaultRequestHeaders.Add(Constants.Headers.XconsentUserId,
                        requireConsentUserId);
            }

            using var response = await httpClient.PostAsync(_configuration["AuthGW:AuthorizeURL"], null);
            response.EnsureSuccessStatusCode();

            return await JsonSerializer.DeserializeAsync<AuthorizeResponse>(await response.Content.ReadAsStreamAsync()) ??
                   throw new NotAuthorizedException("AuthGW could not verify token");
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning("AuthGW could not verify token");
            throw new NotAuthorizedException(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning("AuthGW could not verify token");
            throw new NotAuthorizedException(e.Message);
        }
    }
}
