using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Models;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AuthenticationService
{
    private readonly UsersDbContext _usersDbContext;
    private readonly AnalyticsService<AuthenticationService> _logger;
    private readonly IApplicationSecurity _applicationSecurity;

    public AuthenticationService(UsersDbContext usersDbContext, AnalyticsServiceFactory loggerFactory, IApplicationSecurity applicationSecurity)
    {
        _usersDbContext = usersDbContext;
        _logger = loggerFactory.CreateAnalyticsService<AuthenticationService>();
        _applicationSecurity = applicationSecurity;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="verifyTermsOfServiceAgreement"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    /// <exception cref="NotAuthorizedException">If the access was restricted by security constraints.</exception>
    public async Task<RequestAuthenticatedUser> Authenticate(HttpContext context, bool verifyTermsOfServiceAgreement = true, CancellationToken cancellationToken = default)
    {
        var requestAuthenticationCandinate = await ResolveAuthenticationCandinate(context);

        try
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == requestAuthenticationCandinate.IdentityId && o.Issuer == requestAuthenticationCandinate.Issuer, cancellationToken);
            var person = await _usersDbContext.Persons.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
            if (verifyTermsOfServiceAgreement) await _applicationSecurity.VerifyPersonTermsOfServiceAgreement(person.Id);

            var requestAuthenticatedUser = new RequestAuthenticatedUser(person, requestAuthenticationCandinate);

            context.Items.Add("User", requestAuthenticatedUser);

            return requestAuthenticatedUser;
        }
        catch (InvalidOperationException)
        {
            throw new NotFoundException("Person not found");
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

            _logger.LogAuditLogEvent(AuditLogEvent.Create, authenticatedUser, "AuthenticationService::AuthenticateAndGetOrRegisterAndGetPerson");

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
        requestAuthenticationCandinate.TraceId = context.TraceIdentifier;
        return requestAuthenticationCandinate;
    }
}
