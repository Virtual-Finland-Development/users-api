using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

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
                return null;
            }

            var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);

            return new User(dbUser.Id, dbUser.FirstName, dbUser.LastName);
        }
    }

    public record User(Guid Id, string? FirstName, string? LastName);
    
}