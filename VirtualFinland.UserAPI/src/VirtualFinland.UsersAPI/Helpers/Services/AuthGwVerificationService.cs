using System.Net.Http.Headers;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AuthGwVerificationService
{
    private readonly ILogger<AuthGwVerificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserSecurityService _userSecurityService;

    public AuthGwVerificationService(ILogger<AuthGwVerificationService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, UserSecurityService userSecurityService)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _userSecurityService = userSecurityService;
    }
    
    public async Task<Guid?> GetCurrentUserId(HttpRequest httpRequest)
    {
        Guid? currentUserId = null;
        if (currentUserId == null)
        {
            var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
            
            if (!String.IsNullOrEmpty(token))
            {
                var dbUser = await _userSecurityService.VerifyAndGetAuthenticatedUser(token);
                currentUserId = dbUser?.Id;
            }
        }
        return currentUserId;
    }

    public async Task AuthGwVerification(HttpRequest request)
    {
        try
        {
            var token = request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            httpClient.DefaultRequestHeaders.Add(Constants.Headers.XAuthorizationContext, Constants.Web.AuthGwApplicationContext);
            using HttpResponseMessage response = await httpClient.PostAsync(_configuration["AuthGW:AuthorizeURL"], null);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning("AuthGW could verify token.");
            throw new NotAuthorizedException(e.Message);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning("AuthGW could verify token.");
            throw new NotAuthorizedException(e.Message);
        }
    }
}