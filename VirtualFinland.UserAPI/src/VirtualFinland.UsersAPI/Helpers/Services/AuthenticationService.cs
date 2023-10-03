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

    public async Task<AuthenticatedUser> Authenticate(HttpContext context)
    {
        var jwtTokenResult = await ParseJwtToken(context);

        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == jwtTokenResult.IdentityId && o.Issuer == jwtTokenResult.Issuer, CancellationToken.None);
            var person = await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);

            var authenticatedUser = new AuthenticatedUser(person, jwtTokenResult);
            context.Items.Add("AuthenticatedUser", authenticatedUser);
            return authenticatedUser;
        }
        catch (InvalidOperationException e)
        {
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }

    public async Task<Person> AuthenticateAndGetOrRegisterAndGetPerson(HttpContext context)
    {
        var jwtTokenResult = await ParseJwtToken(context);

        var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(
            o => o.IdentityId == jwtTokenResult.IdentityId && o.Issuer == jwtTokenResult.Issuer, CancellationToken.None);

        // Create a new system user if no one found based on given authentication information
        if (externalIdentity is null)
        {
            var newDbPerson = await _usersDbContext.Persons.AddAsync(
                new Person { Created = DateTime.UtcNow, Modified = DateTime.UtcNow }, CancellationToken.None);

            await _usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity
            {
                Issuer = jwtTokenResult.Issuer,
                IdentityId = jwtTokenResult.IdentityId,
                UserId = newDbPerson.Entity.Id,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }, CancellationToken.None);


            await _usersDbContext.SaveChangesAsync(CancellationToken.None);

            context.Items.Add("AuthenticatedUser", new AuthenticatedUser(newDbPerson.Entity, jwtTokenResult));

            return newDbPerson.Entity;
        }

        var person = await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);

        context.Items.Add("AuthenticatedUser", new AuthenticatedUser(person, jwtTokenResult));

        return person;
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id
    /// </summary>
    private Task<JwtTokenResult> ParseJwtToken(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        return _applicationSecurity.ParseJwtToken(token);
    }
}
