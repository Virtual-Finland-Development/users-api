using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

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
        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }
        
        public async Task<SearchProfile> Handle(Query request, CancellationToken cancellationToken)
        {
            var authenticatedUser = await GetAuthenticatedUser(request, cancellationToken);

            var userSearchProfile = await _usersDbContext.SearchProfiles.SingleOrDefaultAsync(o => o.UserId == authenticatedUser.Id && o.Id == request.ProfileId, cancellationToken);

            if (userSearchProfile is null)
            {
                return null;
            }

            return new SearchProfile(userSearchProfile.Id, userSearchProfile.JobTitles, userSearchProfile.Municipality, userSearchProfile.Name, userSearchProfile.Regions);
        }

        async private Task<Models.User> GetAuthenticatedUser(Query request, CancellationToken cancellationToken)
        {
            // TODO: Better error handling
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);

            if (externalIdentity is null)
            {
                return null;
            }

            return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
        }
    }
    
    public record SearchProfile(Guid Id, string? JobTitles, string? Municipality, string? Name, string? Regions);
}

