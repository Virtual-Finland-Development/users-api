using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class GetConsents
{
    [SwaggerSchema(Title = "ConsentsRequest")]
    public class Query : IRequest<Consents>
    {
        public string? ClaimsUserId { get; }
        public string? ClaimsIssuer { get; }
        
        public Query(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsUserId = claimsUserId;
            this.ClaimsIssuer = claimsIssuer;
        }
    }

    public class Handler : IRequestHandler<Query, Consents>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<Consents> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await GetAuthenticatedUser(request, cancellationToken);
            _logger.LogDebug("User consents retrieved for user: {DbUserId}", dbUser.Id);
            
            return new Consents(
                dbUser.ImmigrationDataConsent,
                dbUser.JobsDataConsent
                );
        }
        
        private async Task<Models.UsersDatabase.User> GetAuthenticatedUser(Query request, CancellationToken cancellationToken)
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
    
    [SwaggerSchema(Title = "ConsentsResponse")]
    public record Consents(
        bool ImmigrationDataConsent,
        bool JobsDataConsent
        );
    
}