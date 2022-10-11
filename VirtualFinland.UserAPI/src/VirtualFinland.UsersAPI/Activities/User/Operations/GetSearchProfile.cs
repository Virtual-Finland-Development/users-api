using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class GetSearchProfile
{
    public class Query : IRequest<SearchProfile>
    {
        public string? ClaimsUserId { get; }
        public string? ClaimsIssuer { get; }
        
        public Guid ProfileId { get; }

        public Query(string? claimsUserId, string? claimsIssuer, Guid profileId)
        {
            this.ClaimsUserId = claimsUserId;
            this.ClaimsIssuer = claimsIssuer;
            this.ProfileId = profileId;
        }
    }

    public class Handler : IRequestHandler<Query, SearchProfile>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<SearchProfile> Handle(Query request, CancellationToken cancellationToken)
        {
            var authenticatedUser = await GetAuthenticatedUser(request, cancellationToken);

            var userSearchProfile = await _usersDbContext.SearchProfiles.SingleOrDefaultAsync(o => o.UserId == authenticatedUser.Id && o.Id == request.ProfileId, cancellationToken);

            if (userSearchProfile is null)
            {
                _logger.LogInformation("Failed to retrieve search profile: {RequestProfileId}", request.ProfileId);
                throw new NotFoundException($"Specified search profile not found by ID: {request.ProfileId}");
            }
            
            _logger.LogDebug("Search Profile Retrieved: {SearchProfileId}", userSearchProfile.Id);

            return new SearchProfile(userSearchProfile.Id, userSearchProfile.JobTitles, userSearchProfile.Name, userSearchProfile.Regions, userSearchProfile.Created, userSearchProfile.Modified);
        }

        async private Task<Models.User> GetAuthenticatedUser(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);
                return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", request.ClaimsUserId, request.ClaimsIssuer);
                throw new NotAuthorizedExpception("User could not be identified as a valid user.", e);
            }
        }
    }
    
    [SwaggerSchema("SearchProfile")]
    public record SearchProfile(Guid Id, List<string>? JobTitles, string? Name, List<string>? Regions, DateTime Created, DateTime Modified);
}

