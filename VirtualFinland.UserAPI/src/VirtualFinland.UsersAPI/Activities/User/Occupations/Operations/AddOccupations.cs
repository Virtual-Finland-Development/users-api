using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Occupations.Operations;

public static class AddOccupations
{
    public class Command : IRequest<List<Occupation>>
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

    public class Handler : IRequestHandler<Command, List<Occupation>>
    {
        private readonly UsersDbContext _usersDbContext;

        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }

        public async Task<List<Occupation>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = _usersDbContext.Users
                .Include(u => u.Occupations)
                .FirstOrDefault(u => u.Id == request.UserId);

            foreach (var occupation in request.Occupations)
            {
                var newOccupation = new Models.UsersDatabase.Occupation
                {
                    NaceCode = occupation.NaceCode,
                    EscoCode = occupation.EscoCode,
                    EscoUri = occupation.EscoUri,
                    WorkMonths = occupation.WorkMonths
                };
                user?.Occupations?.Add(newOccupation);  
            }
            
            var addedEntries = _usersDbContext.ChangeTracker
                .Entries()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .ToList();
            
            await _usersDbContext.SaveChangesAsync(cancellationToken);

            var addedOccupations = new List<Occupation>();
            foreach (Models.UsersDatabase.Occupation entry in addedEntries)
            {
                addedOccupations.Add(new Occupation
                {
                    Id = entry.Id,
                    NaceCode = entry.NaceCode,
                    EscoUri = entry.EscoUri,
                    EscoCode = entry.EscoCode,
                    WorkMonths = entry.WorkMonths
                });
            }
            
            return addedOccupations;
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
