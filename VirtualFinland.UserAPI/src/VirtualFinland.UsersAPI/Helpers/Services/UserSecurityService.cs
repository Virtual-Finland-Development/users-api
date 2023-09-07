using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class UserSecurityService
{
    private readonly UsersDbContext _usersDbContext;
    private readonly IApplicationSecurity _applicationSecurity;

    public UserSecurityService(UsersDbContext usersDbContext, IApplicationSecurity applicationSecurity)
    {
        _usersDbContext = usersDbContext;
        _applicationSecurity = applicationSecurity;
    }

    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    /// <exception cref="NotAuthorizedException">If the access was restricted by security constraints.</exception>
    public async Task<Person> VerifyAndGetAuthenticatedUser(string token)
    {
        var jwtTokenResult = ParseJwtToken(token);

        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == jwtTokenResult.UserId && o.Issuer == jwtTokenResult.Issuer, CancellationToken.None);
            return await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            throw new NotFoundException("Person not found");
        }
    }

    public async Task VerifyPersonTermsOfServiceAgreement(Guid personId)
    {
        await _applicationSecurity.VerifyPersonTermsOfServiceAgreement(personId);
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id
    /// </summary>
    public JwtTokenResult ParseJwtToken(string token)
    {
        return _applicationSecurity.ParseJwtToken(token);
    }
}
