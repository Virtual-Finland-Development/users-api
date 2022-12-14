using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Occupations.Operations;

public static class UpdateOccupations
{
    public class Command : IRequest
    {
        public Command(List<Occupation> occupations)
        {
            Occupations = occupations;
        }

        [SwaggerIgnore] public Guid? UserId { get; private set; }
        public List<Occupation> Occupations { get; init; }

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
            var userOccupationsToUpdate = await _usersDbContext.Occupations
                .Where(o => o.UserId == request.UserId)
                .Where(o => request.Occupations.Select(e => e.Id).Contains(o.Id))
                .ToListAsync(cancellationToken);

            if (userOccupationsToUpdate is { Count: 0 }) throw new NotFoundException();

            foreach (var occupation in request.Occupations)
            {
                var editable = userOccupationsToUpdate.First(o => o.Id == occupation.Id);
                editable.NaceCode = occupation.NaceCode ?? editable.NaceCode;
                editable.EscoUri = occupation.EscoUri ?? editable.EscoUri;
                editable.EscoCode = occupation.EscoCode ?? editable.EscoCode;
                editable.WorkMonths = occupation.WorkMonths ?? editable.WorkMonths;
            }

            await _usersDbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    public record Occupation
    {
        public Guid Id { get; init; }
        [MaxLength(7)] public string? NaceCode { get; init; }
        [Url] public string? EscoUri { get; init; }
        [MaxLength(16)] public string? EscoCode { get; init; }
        [Range(0, 600)] public int? WorkMonths { get; init; }
    }
}
