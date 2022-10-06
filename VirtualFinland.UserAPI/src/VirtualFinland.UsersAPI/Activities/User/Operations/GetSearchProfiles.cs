using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class GetSearchProfiles
{
    public class Query : IRequest<IList<SearchProfile>>
    {
        public string? ClaimsUserId { get; }
        public string? ClaimsIssuer { get; }
        
        public Query(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsUserId = claimsUserId;
            this.ClaimsIssuer = claimsIssuer;
        }
    }

    public class Handler : IRequestHandler<Query, IList<SearchProfile>>
    {
        private readonly UsersDbContext _usersDbContext;
        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }
        
        public async Task<IList<SearchProfile>> Handle(Query request, CancellationToken cancellationToken)
        {
            var authenticatedUser = await GetAuthenticatedUser(request, cancellationToken);

            var userSearchProfiles = _usersDbContext.SearchProfiles.Where(o => o.UserId == authenticatedUser.Id);

            return await userSearchProfiles.Select(o => new SearchProfile(o.Id, o.JobTitles, o.Municipality, o.Name, o.Regions)).ToListAsync(cancellationToken);
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
    
    [SwaggerSchema(Title = "SearchProfile")]

    public record SearchProfile(Guid Id, string JobTitles, string Municipality, string Name, string? Regions);
}

