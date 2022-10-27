using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class UpdateConsents
{
    [SwaggerSchema(Title = "UpdateConsentsRequest")]
    public class Command : IRequest<Consents>
    {
        public bool? JobsDataConsent { get; set; }
        
        public bool? ImmigrationDataConsent { get; set; }

        [SwaggerIgnore]
        public string? ClaimsUserId { get; set; }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public Command(bool? jobsDataConsent, bool? immigrationDataConsent)
        {
            this.JobsDataConsent = jobsDataConsent;
            this.ImmigrationDataConsent = immigrationDataConsent;
        }

        public void SetAuth(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsIssuer = claimsIssuer;
            this.ClaimsUserId = claimsUserId;
        }
    }

    public class Handler : IRequestHandler<Command, Consents>
        {
            private readonly UsersDbContext _usersDbContext;
            private readonly ILogger<Handler> _logger;

            public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
            {
                _usersDbContext = usersDbContext;
                _logger = logger;
            }

            public async Task<Consents> Handle(Command request, CancellationToken cancellationToken)
            {
                var dbUser = await GetAuthenticatedUser(request, cancellationToken);
                
                dbUser.Modified = DateTime.UtcNow;
                dbUser.ImmigrationDataConsent = request.ImmigrationDataConsent ?? dbUser.ImmigrationDataConsent;
                dbUser.JobsDataConsent = request.JobsDataConsent ?? dbUser.JobsDataConsent;
                
                await _usersDbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("User data updated for user: {DbUserId}", dbUser.Id);

                return new Consents(
                    dbUser.ImmigrationDataConsent,
                    dbUser.JobsDataConsent);
            }

            private async Task<Models.UsersDatabase.User> GetAuthenticatedUser(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);
                    return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", request.ClaimsUserId, request.ClaimsIssuer);
                    throw new NotAuthorizedException("User could not be identified as a valid user.", e);
                }
            }
            
            
        }
    [SwaggerSchema(Title = "UpdateConsentsResponse")]
    public record Consents(
        bool ImmigrationDataConsent,
        bool JobsDataConsent);
}