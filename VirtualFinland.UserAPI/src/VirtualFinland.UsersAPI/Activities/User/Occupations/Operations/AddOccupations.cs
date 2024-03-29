using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Occupations.Operations;

public static class AddOccupations
{
    public class Command : AuthenticatedRequest<List<AddOccupationsResponse>>
    {
        public Command(List<AddOccupationsRequest> occupations)
        {
            Occupations = occupations;
        }

        public List<AddOccupationsRequest> Occupations { get; init; }
    }

    public class Handler : IRequestHandler<Command, List<AddOccupationsResponse>>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly AnalyticsLogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, AnalyticsLoggerFactory loggerFactory)
        {
            _usersDbContext = usersDbContext;
            _logger = loggerFactory.CreateAnalyticsLogger<Handler>();
        }

        public async Task<List<AddOccupationsResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = _usersDbContext.Persons
                .Include(u => u.Occupations)
                .FirstOrDefault(u => u.Id == request.User.PersonId);

            foreach (var occupation in request.Occupations)
            {
                var newOccupation = new Models.UsersDatabase.Occupation
                {
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

            await _usersDbContext.SaveChangesAsync(request.User, cancellationToken);
            await _logger.LogAuditLogEvent(AuditLogEvent.Update, request.User);

            var addedOccupations = new List<AddOccupationsResponse>();
            foreach (Models.UsersDatabase.Occupation entry in addedEntries)
            {
                addedOccupations.Add(new AddOccupationsResponse
                {
                    Id = entry.Id,
                    EscoUri = entry.EscoUri,
                    EscoCode = entry.EscoCode,
                    WorkMonths = entry.WorkMonths
                });
            }

            return addedOccupations;
        }
    }

    [SwaggerSchema(Title = "AddOccupationsResponse")]
    public record AddOccupationsResponse
    {
        public Guid Id { get; init; }

        [Url]
        public string? EscoUri { get; init; }

        [MaxLength(16)]
        public string? EscoCode { get; init; }

        [Range(0, 600)]
        public int? WorkMonths { get; init; }
    }

    [SwaggerSchema(Title = "AddOccupationsRequest")]
    public record AddOccupationsRequest
    {
        [Url]
        public string? EscoUri { get; init; }

        [MaxLength(16)]
        public string? EscoCode { get; init; }

        [Range(0, 600)]
        public int? WorkMonths { get; init; }
    }
}