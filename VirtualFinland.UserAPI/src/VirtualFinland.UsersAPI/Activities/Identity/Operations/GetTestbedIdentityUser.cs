using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models;

namespace VirtualFinland.UserAPI.Activities.Identity.Operations;

public class GetTestbedIdentityUser
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

    public class GetTestbedIdentityUserHandler : IRequestHandler<Query, User>
    {
        private readonly UsersDbContext _usersDbContext;
        public GetTestbedIdentityUserHandler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }
        
        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            // TODO: Better response error and exception handling
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);

            // Create a new system user is no one found based on given authentication information

            if (externalIdentity is null)
            {
                var newDbUSer = await _usersDbContext.Users.AddAsync(new Models.User()
                {
                    TestbedId = request.ClaimsUserId
                }, cancellationToken);

                var newExternalIdentity = await _usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity()
                {
                    Issuer = request.ClaimsIssuer,
                    IdentityId = request.ClaimsUserId,
                    UserId = newDbUSer.Entity.Id
                }, cancellationToken);
                

                await _usersDbContext.SaveChangesAsync(cancellationToken);
                return new User(newDbUSer.Entity.Id);
            }
            
            var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
            return new User(dbUser.Id);
        }
    }

    public record User(Guid Id);
}