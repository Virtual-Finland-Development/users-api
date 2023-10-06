using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class DeleteUser
{
    public class Command : AuthenticatedRequest
    {

    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly UsersDbContext _context;

        public Handler(UsersDbContext context)
        {
            _context = context;
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
            person.SetupAuditEvents(_context, request.User);

            try
            {
                _context.Persons.Remove(person);

                if (externalIdentity != null)
                {
                    _context.ExternalIdentities.Remove(externalIdentity);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
            }


            return Unit.Value;
        }
    }
}
