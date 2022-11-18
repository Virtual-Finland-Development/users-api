using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AuthGwVerificationService
{
    private readonly ILogger<AuthGwVerificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UsersDbContext _usersDbContext;

    public AuthGwVerificationService(ILogger<AuthGwVerificationService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, UsersDbContext usersDbContext)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _usersDbContext = usersDbContext;
    }
    
    public async Task<Guid?> GetCurrentUserId(HttpRequest httpRequest)
    {
        Guid? currentUserId = null;
        if (currentUserId == null)
        {
            var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
            var issuer = this.GetIssuer(token);
            var userId = this.GetUserId(token);
            
            if (!String.IsNullOrEmpty(issuer) && !String.IsNullOrEmpty(userId))
            {
                var dbUser = await VerifyAndGetAuthenticatedUser(issuer, userId);
                currentUserId = dbUser?.Id;
            }
        }
        return currentUserId;
    }

    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="claimsIssuer"></param>
    /// <param name="claimsUserId"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    private async Task<Models.UsersDatabase.User?> VerifyAndGetAuthenticatedUser(string? claimsIssuer, string? claimsUserId)
    {
        try
        {
            if (claimsIssuer != null && claimsIssuer.Contains(_configuration["AuthGW:Issuer"]))
            {
                claimsUserId = "suomifiDummyUserId";
            }
            
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == claimsUserId && o.Issuer == claimsIssuer, CancellationToken.None);
            return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", claimsUserId, claimsIssuer);
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
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

    private String? GetUserId(String token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return String.Empty;
        }

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
        return String.IsNullOrEmpty(jwtSecurityToken.Subject) ? jwtSecurityToken.Claims.FirstOrDefault(o => o.Type == "userId")?.Value : jwtSecurityToken.Subject;
    }

    private String? GetIssuer(String token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var canReadToken = tokenHandler.CanReadToken(token);
        return canReadToken ? tokenHandler.ReadJwtToken(token).Issuer : string.Empty;
    }
}