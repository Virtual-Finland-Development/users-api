using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class DeleteUser
{
    public class Command : AuthenticatedRequest
    {

    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly UsersDbContext _context;
        private readonly AnalyticsService _logger;

        public Handler(UsersDbContext context, AnalyticsService logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var person = await _context.Persons
                .Include(p => p.Occupations)
                .Include(p => p.Educations)
                .Include(p => p.LanguageSkills)
                .Include(p => p.Skills)
                .Include(p => p.Certifications)
                .Include(p => p.Permits)
                .Include(p => p.WorkPreferences)
                .SingleAsync(p => p.Id == request.User.PersonId, cancellationToken);
            var externalIdentity = await _context.ExternalIdentities.SingleOrDefaultAsync(id => id.UserId == request.User.PersonId);

            try
            {
                _context.Persons.Remove(person);

                if (externalIdentity != null)
                {
                    _context.ExternalIdentities.Remove(externalIdentity);
                }

                await _context.SaveChangesAsync(request.User, cancellationToken);

                _logger.LogAuditLogEvent(AuditLogEvent.Delete, "Person", request.User);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
            }


            return Unit.Value;
        }
    }
}
