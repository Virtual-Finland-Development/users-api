using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Occupations.Operations;

public static class AddOccupation
{
    public class Command : IRequest<Occupation>
    {
        public Command(string? naceCode, string? escoUri, string? escoCode, int? workMonths)
        {
            NaceCode = naceCode;
            EscoUri = escoUri;
            EscoCode = escoCode;
            WorkMonths = workMonths;
        }

        [SwaggerIgnore] public Guid? UserId { get; private set; }
        [MaxLength(7)] public string? NaceCode { get; init; }
        [Url] public string? EscoUri { get; init; }
        [MaxLength(16)] public string? EscoCode { get; init; }
        [Range(0, 600)] public int? WorkMonths { get; init; }


        public void SetAuth(Guid? userDatabaseIdentifier)
        {
            UserId = userDatabaseIdentifier;
        }
    }

    public class Handler : IRequestHandler<Command, Occupation>
    {
        private readonly UsersDbContext _usersDbContext;

        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }

        public async Task<Occupation> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = _usersDbContext.Users
                .Include(u => u.Occupations)
                .FirstOrDefault(u => u.Id == request.UserId);

            var occupationToAdd = new Models.UsersDatabase.Occupation
            {
                NaceCode = request.NaceCode,
                EscoCode = request.EscoCode,
                EscoUri = request.EscoUri,
                WorkMonths = request.WorkMonths
            };

            user?.Occupations?.Add(occupationToAdd);

            await _usersDbContext.SaveChangesAsync(cancellationToken);

            return new Occupation
            {
                Id = occupationToAdd.Id,
                NaceCode = occupationToAdd.NaceCode,
                EscoUri = occupationToAdd.EscoUri,
                EscoCode = occupationToAdd.EscoCode,
                WorkMonths = occupationToAdd.WorkMonths
            };
        }
    }

    public record Occupation
    {
        public Guid Id { get; set; }
        [MaxLength(7)] public string? NaceCode { get; set; }
        [Url] public string? EscoUri { get; set; }
        [MaxLength(16)] public string? EscoCode { get; set; }
        [Range(0, 600)] public int? WorkMonths { get; set; }
    }
}
