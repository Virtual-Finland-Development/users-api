using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class UserSecurityService
{
    private readonly UsersDbContext _usersDbContext;
    private readonly ILogger<UserSecurityService> _logger;
    private readonly IApplicationSecurity _applicationSecurity;

    public UserSecurityService(UsersDbContext usersDbContext, ILogger<UserSecurityService> logger, IApplicationSecurity applicationSecurity)
    {
        _usersDbContext = usersDbContext;
        _logger = logger;
        _applicationSecurity = applicationSecurity;
    }

    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    public async Task<Person> VerifyAndGetAuthenticatedUser(string token)
    {
        var jwtTokenResult = await ParseJwtToken(token);

        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == jwtTokenResult.UserId && o.Issuer == jwtTokenResult.Issuer, CancellationToken.None);
            return await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", jwtTokenResult.UserId, jwtTokenResult.Issuer);
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id
    /// </summary>
    public Task<JwtTokenResult> ParseJwtToken(string token)
    {
        return _applicationSecurity.ParseJwtToken(token);
    }
}
