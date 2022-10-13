using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class GetUser
{
    [SwaggerSchema(Title = "UserRequest")]
    public class Query : IRequest<User>
    {
        public string? ClaimsUserId { get; }
        public string? ClaimsIssuer { get; }
        
        public Query(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsUserId = claimsUserId;
            this.ClaimsIssuer = claimsIssuer;
        }

    }

    public class Handler : IRequestHandler<Query, User>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await GetAuthenticatedUser(request, cancellationToken);

            // TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);
            _logger.LogDebug("User data retrieved for user: {DbUserId}", dbUser.Id);
            
            return new User(dbUser.Id, dbUser.FirstName, dbUser.LastName, dbUser.Address, dbUserDefaultSearchProfile?.JobTitles, dbUserDefaultSearchProfile?.Regions, dbUser.Created, dbUser.Modified);
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
                throw new NotAuthorizedException("User could not be identified as a valid user.", e);
            }
        }
    }
    
    [SwaggerSchema(Title = "UserResponse")]
    public record User(Guid Id, string? FirstName, string? LastName, string? address, List<string>? JobTitles, List<string>? Regions, DateTime Created, DateTime Modified);
    
}