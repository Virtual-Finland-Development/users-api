using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AuthGwVerificationService
{
    private readonly ILogger<AuthGwVerificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public AuthGwVerificationService(ILogger<AuthGwVerificationService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task AuthGwVerification(HttpRequest request)
    {
        try
        {
            var token = request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
            var issuer = this.GetIssuer(token);

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            httpClient.DefaultRequestHeaders.Add(Constants.Headers.XAuthorizationContext, Constants.Web.AuthGwApplicationContext);
            httpClient.DefaultRequestHeaders.Add(Constants.Headers.XAuthorizationProvider, issuer);
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

    private String GetIssuer(String token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var canReadToken = tokenHandler.CanReadToken(token);
        var issuer = canReadToken ? tokenHandler.ReadJwtToken(token).Issuer : string.Empty;
        return issuer;
    }
}