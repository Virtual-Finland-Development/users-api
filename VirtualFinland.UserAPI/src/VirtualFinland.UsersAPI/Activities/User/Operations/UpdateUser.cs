using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class UpdateUser
{
    [SwaggerSchema(Title = "UpdateUser")]
    public class UpdateUserCommand : IRequest
    {
        public Guid Id { get; }
        public string? FirstName { get; }
        public string? LastName { get; }
        public UpdateUserCommand(Guid id, string firstName, string lastName)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
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
                var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == request.Id, cancellationToken);

                dbUser.FirstName = request.FirstName ?? dbUser.FirstName;
                dbUser.LastName = request.LastName ?? dbUser.LastName;

                await _usersDbContext.SaveChangesAsync(cancellationToken);

                return await Unit.Task;
            }
        }
    }
}