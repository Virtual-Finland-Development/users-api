using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinlandDevelopment.Shared.Exceptions;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class DeleteUser
{
    public class Command : IRequest
    {
        public Command()
        { }

        [SwaggerIgnore]
        public Guid? UserId { get; set; }

        public void SetAuth(Guid? userDatabaseId)
        {
            UserId = userDatabaseId;
        }
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
                .SingleAsync(p => p.Id == request.UserId, cancellationToken);
            var externalIdentity = await _context.ExternalIdentities.SingleOrDefaultAsync(id => id.UserId == request.UserId);

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
