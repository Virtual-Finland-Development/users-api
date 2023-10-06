using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AuthenticationService
{
    private readonly UsersDbContext _usersDbContext;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IApplicationSecurity _applicationSecurity;

    public AuthenticationService(UsersDbContext usersDbContext, ILogger<AuthenticationService> logger, IApplicationSecurity applicationSecurity)
    {
        _usersDbContext = usersDbContext;
        _logger = logger;
        _applicationSecurity = applicationSecurity;
    }

    public async Task<RequestAuthenticatedUser> Authenticate(HttpContext context, CancellationToken cancellationToken = default)
    {
        var requestAuthenticationCandinate = await ParseJwtToken(context);

        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == requestAuthenticationCandinate.IdentityId && o.Issuer == requestAuthenticationCandinate.Issuer, CancellationToken.None);
            var person = await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

            var requestAuthenticatedUser = new RequestAuthenticatedUser(person, requestAuthenticationCandinate);

            context.Items.Add("User", requestAuthenticatedUser);

            return requestAuthenticatedUser;
        }
        catch (InvalidOperationException e)
        {
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }

    public async Task<Person> AuthenticateAndGetOrRegisterAndGetPerson(HttpContext context, CancellationToken cancellationToken = default)
    {
        var requestAuthenticationCandinate = await ParseJwtToken(context);

        var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(
            o => o.IdentityId == requestAuthenticationCandinate.IdentityId && o.Issuer == requestAuthenticationCandinate.Issuer, cancellationToken);

        // Create a new system user if no one found based on given authentication information
        if (externalIdentity is null)
        {
            var newDbPerson = await _usersDbContext.Persons.AddAsync(
               new Person().MakeAuditAddition(requestAuthenticationCandinate), cancellationToken
            );

            await _usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity
            {
                Issuer = requestAuthenticationCandinate.Issuer,
                IdentityId = requestAuthenticationCandinate.IdentityId,
                UserId = newDbPerson.Entity.Id,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }, cancellationToken);

            await _usersDbContext.SaveChangesAsync(cancellationToken);

            context.Items.Add("User", new RequestAuthenticatedUser(newDbPerson.Entity, requestAuthenticationCandinate));

            return newDbPerson.Entity;
        }

        var person = await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

        context.Items.Add("User", new RequestAuthenticatedUser(person, requestAuthenticationCandinate));

        return person;
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id
    /// </summary>
    private Task<RequestAuthenticationCandinate> ParseJwtToken(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        return _applicationSecurity.ParseJwtToken(token);
    }
}
