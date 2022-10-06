using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class GetUser
{
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
        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }
        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);

            if (externalIdentity is null)
            {
                throw new NotAuthorizedExpception("User could not be identified as a valid user.");
            }

            var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

            return new User(dbUser.Id, dbUser.FirstName, dbUser.LastName);
        }
    }
    
    [SwaggerSchema("User")]
    public record User(Guid Id, string? FirstName, string? LastName);
    
}