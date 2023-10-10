using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Helpers.Extensions;

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
        var requestAuthenticationCandinate = await ResolveAuthenticationCandinate(context);

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
            throw new NotAuthorizedException("User could not be identified as a valid user", e);
        }
    }

    public async Task<Person> AuthenticateAndGetOrRegisterAndGetPerson(HttpContext context, CancellationToken cancellationToken = default)
    {
        var requestAuthenticationCandinate = await ResolveAuthenticationCandinate(context);

        var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(
            o => o.IdentityId == requestAuthenticationCandinate.IdentityId && o.Issuer == requestAuthenticationCandinate.Issuer, cancellationToken);

        // Create a new system user if no one found based on given authentication information
        if (externalIdentity is null)
        {
            var newDbPerson = await _usersDbContext.Persons.AddAsync(
               new Person(), cancellationToken
            );

            await _usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity
            {
                Issuer = requestAuthenticationCandinate.Issuer,
                IdentityId = requestAuthenticationCandinate.IdentityId,
                UserId = newDbPerson.Entity.Id,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }, cancellationToken);

            await _usersDbContext.SaveChangesAsync(requestAuthenticationCandinate, cancellationToken);

            var authenticatedUser = new RequestAuthenticatedUser(newDbPerson.Entity, requestAuthenticationCandinate);

            _logger.LogAuditLogEvent(AuditLogEvent.Create, authenticatedUser);

            context.Items.Add("User", authenticatedUser);

            return newDbPerson.Entity;
        }

        var person = await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

        context.Items.Add("User", new RequestAuthenticatedUser(person, requestAuthenticationCandinate));

        return person;
    }

    /// <summary>
    /// Parses the JWT token and returns the authentication candinate
    /// </summary>
    private async Task<RequestAuthenticationCandinate> ResolveAuthenticationCandinate(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        var requestAuthenticationCandinate = await _applicationSecurity.ParseJwtToken(token);
        requestAuthenticationCandinate.TraceId = context.Request.Headers[Constants.Headers.XRequestTraceId].ToString();
        return requestAuthenticationCandinate;
    }
}
