using System.Net.Http.Headers;
using VirtualFinland.UserAPI.Exceptions;

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
    /// <param name="requireConsentToken">If true, pass the consent token to the AuthGW as a verification requirement</param>
    public async Task VerifyTokens(HttpRequest request, bool requireConsentToken = false)
    {
        try
        {
            var token = request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            httpClient.DefaultRequestHeaders.Add(
                Constants.Headers.XAuthorizationContext,
                Constants.Web.AuthGwApplicationContext);

            if (requireConsentToken)
            {
                if (!request.Headers.ContainsKey(Constants.Headers.XConsentToken))
                    throw new NotAuthorizedException("Consent token is missing.");

                httpClient.DefaultRequestHeaders.Add(
                    Constants.Headers.XConsentToken,
                    request.Headers[Constants.Headers.XConsentToken].ToString());
            }

            using var response = await httpClient.PostAsync(_configuration["AuthGW:AuthorizeURL"], null);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning("AuthGW could verify token");
            throw new NotAuthorizedException(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning("AuthGW could verify token");
            throw new NotAuthorizedException(e.Message);
        }
    }
}
