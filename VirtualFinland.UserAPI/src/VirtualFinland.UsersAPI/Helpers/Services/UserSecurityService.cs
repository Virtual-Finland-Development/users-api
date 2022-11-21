using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class UserSecurityService
{
    private readonly UsersDbContext _usersDbContext;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;

    public UserSecurityService(UsersDbContext usersDbContext, ILogger<AuthenticationService> logger, IConfiguration configuration)
    {
        _usersDbContext = usersDbContext;
        _logger = logger;
        _configuration = configuration;
    }
    
    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="claimsIssuer"></param>
    /// <param name="claimsUserId"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    public async Task<Models.UsersDatabase.User?> VerifyAndGetAuthenticatedUser(string token)
    {
        var issuer = this.GetTokenIssuer(token);
        var userId = this.GetTokenUserId(token);
        
        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == userId && o.Issuer == issuer, CancellationToken.None);
            return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", userId, issuer);
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }
    
    public String? GetTokenUserId(String token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return String.Empty;
        }

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
        return String.IsNullOrEmpty(jwtSecurityToken.Subject) ? jwtSecurityToken.Claims.FirstOrDefault(o => o.Type == "userId")?.Value : jwtSecurityToken.Subject;
    }

    public String? GetTokenIssuer(String token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var canReadToken = tokenHandler.CanReadToken(token);
        return canReadToken ? tokenHandler.ReadJwtToken(token).Issuer : string.Empty;
    }
}