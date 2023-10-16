using MediatR;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Extensions;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Occupations.Operations;

public static class DeleteOccupations
{
    public class Command : AuthenticatedRequest
    {
        public Command()
        {
        }

        public Command(List<Guid> ids)
        {
            Ids = ids;
        }

        public List<Guid> Ids { get; init; } = null!;
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var userOccupations = _usersDbContext.Occupations
                .Where(o => o.PersonId == request.User.PersonId)
                .ToList();

            var occupationsToRemove = request.Ids switch
            {
                { Count: > 0 } => userOccupations.IntersectBy(request.Ids, o => o.Id).ToList(),
                { Count: 0 } => throw new InvalidOperationException(),
                _ => userOccupations
            };

            _usersDbContext.Occupations.RemoveRange(occupationsToRemove);
            await _usersDbContext.SaveChangesAsync(request.User, cancellationToken);
            _logger.LogAuditLogEvent(AuditLogEvent.Update, request.User);

            return Unit.Value;
        }
    }
}
