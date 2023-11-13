using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Occupations.Operations;

public static class UpdateOccupations
{
    public class Command : AuthenticatedRequest
    {
        public Command(List<Occupation> occupations)
        {
            Occupations = occupations;
        }

        public List<Occupation> Occupations { get; init; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly AnalyticsLogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, AnalyticsLoggerFactory loggerFactory)
        {
            _usersDbContext = usersDbContext;
            _logger = loggerFactory.CreateAnalyticsLogger<Handler>();
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var userOccupationsToUpdate = await _usersDbContext.Occupations
                .Where(o => o.PersonId == request.User.PersonId)
                .Where(o => request.Occupations.Select(e => e.Id).Contains(o.Id))
                .ToListAsync(cancellationToken);

            if (userOccupationsToUpdate is { Count: 0 }) throw new NotFoundException();

            foreach (var occupation in request.Occupations)
            {
                var editable = userOccupationsToUpdate.First(o => o.Id == occupation.Id);
                editable.EscoUri = occupation.EscoUri ?? editable.EscoUri;
                editable.EscoCode = occupation.EscoCode ?? editable.EscoCode;
                editable.WorkMonths = occupation.WorkMonths ?? editable.WorkMonths;
            }

            await _usersDbContext.SaveChangesAsync(request.User, cancellationToken);
            await _logger.LogAuditLogEvent(AuditLogEvent.Update, request.User);

            return Unit.Value;
        }
    }

    public record Occupation
    {
        public Guid Id { get; init; }

        [Url]
        public string? EscoUri { get; init; }

        [MaxLength(16)]
        public string? EscoCode { get; init; }

        [Range(0, 600)]
        public int? WorkMonths { get; init; }
    }
}
