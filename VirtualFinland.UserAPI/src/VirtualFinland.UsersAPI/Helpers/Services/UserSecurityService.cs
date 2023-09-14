using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Models;

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
    public async Task<AuthenticatedUser> VerifyAndGetAuthenticatedUser(string token)
    {
        var authUser = await GetAuthenticatedUser(token);

        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == authUser.PersonId.ToString() && o.Issuer == authUser.Issuer, CancellationToken.None);
            var person = await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
            return authUser;
        }
        catch (InvalidOperationException)
        {
            throw new NotFoundException("Person not found");
        }
    }

    /// <summary>
    /// Verifies that the user has accepted the latest terms of service
    /// </summary>
    public async Task VerifyPersonTermsOfServiceAgreement(AuthenticatedUser authenticatedUser)
    {
        await _applicationSecurity.VerifyPersonTermsOfServiceAgreement(authenticatedUser.PersonId, authenticatedUser.Audience);
    }
    public async Task VerifyPersonTermsOfServiceAgreement(Guid personId, string audience)
    {
        await _applicationSecurity.VerifyPersonTermsOfServiceAgreement(personId, audience);
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id, and the audience
    /// </summary>
    public Task<AuthenticatedUser> GetAuthenticatedUser(string token)
    {
        return _applicationSecurity.GetAuthenticatedUser(token);
    }
}
