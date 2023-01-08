using MediatR;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Occupations.Operations;

public static class DeleteOccupations
{
    public class Command : IRequest
    {
        public Command()
        {
        }

        public Command(List<Guid> ids)
        {
            Ids = ids;
        }

        [SwaggerIgnore] public Guid? UserId { get; private set; }

        public List<Guid> Ids { get; init; } = null!;

        public void SetAuth(Guid? userDatabaseIdentifier)
        {
            UserId = userDatabaseIdentifier;
        }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly UsersDbContext _usersDbContext;

        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var userOccupations = _usersDbContext.Occupations
                .Where(o => o.UserId == request.UserId)
                .ToList();

            var occupationsToRemove = request.Ids switch
            {
                { Count: > 0 } => userOccupations.IntersectBy(request.Ids, o => o.Id).ToList(),
                { Count: 0 } => throw new InvalidOperationException(),
                _ => userOccupations
            };

            _usersDbContext.Occupations.RemoveRange(occupationsToRemove);
            await _usersDbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
