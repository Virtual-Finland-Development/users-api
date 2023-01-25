using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class UserSecurityService
{
    private readonly UsersDbContext _usersDbContext;
    private readonly ILogger<UserSecurityService> _logger;

    public UserSecurityService(UsersDbContext usersDbContext, ILogger<UserSecurityService> logger)
    {
        _usersDbContext = usersDbContext;
        _logger = logger;
    }
    
    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    public async Task<Person> VerifyAndGetAuthenticatedUser(string token)
    {
        var issuer = GetTokenIssuer(token);
        var userId = GetTokenUserId(token);
        
        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == userId && o.Issuer == issuer, CancellationToken.None);
            return await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", userId, issuer);
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }

    private static string? GetTokenUserId(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return string.Empty;
        }

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
        return string.IsNullOrEmpty(jwtSecurityToken.Subject) ? jwtSecurityToken.Claims.FirstOrDefault(o => o.Type == "userId")?.Value : jwtSecurityToken.Subject;
    }

    private static string? GetTokenIssuer(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var canReadToken = tokenHandler.CanReadToken(token);
        return canReadToken ? tokenHandler.ReadJwtToken(token).Issuer : string.Empty;
    }
}
