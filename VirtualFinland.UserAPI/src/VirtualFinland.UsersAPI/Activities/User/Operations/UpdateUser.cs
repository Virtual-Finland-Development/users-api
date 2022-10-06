using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class UpdateUser
{
    [SwaggerSchema(Title = "UpdateUser")]
    public class UpdateUserCommand : IRequest
    {
        public Guid Id { get; }
        public string? FirstName { get; }
        public string? LastName { get; }

        [SwaggerIgnore]
        public string? ClaimsUserId { get; set; }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }
        public UpdateUserCommand(Guid id, string firstName, string lastName)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
        }
        
        public void SetAuth(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsIssuer = claimsIssuer;
            this.ClaimsUserId = claimsUserId;
        }
    }

    public class Handler : IRequestHandler<UpdateUserCommand>
        {
            private readonly UsersDbContext _usersDbContext;
            public Handler(UsersDbContext usersDbContext)
            {
                _usersDbContext = usersDbContext;
            }
            public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);

                if (externalIdentity is null)
                {
                    throw new NotAuthorizedExpception("User could not be identified as a valid user.");
                }
                
                var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == request.Id, cancellationToken);

                dbUser.FirstName = request.FirstName ?? dbUser.FirstName;
                dbUser.LastName = request.LastName ?? dbUser.LastName;

                await _usersDbContext.SaveChangesAsync(cancellationToken);

                return await Unit.Task;
            }
        }
}