using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Extensions;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class UpdateSearchProfile
{
    [SwaggerSchema(Title = "UpdateSearchProfileRequest")]
    public class Command : AuthenticatedRequest
    {
        public Guid Id { get; }
        public List<string>? JobTitles { get; }
        public List<string>? Regions { get; }

        public string? Name { get; }

        public Command(Guid id, List<string> jobTitles, List<string> regions, string name)
        {
            this.Id = id;
            this.JobTitles = jobTitles;
            this.Regions = regions;
            this.Name = name;
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.User.PersonId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly AnalyticsService<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, AnalyticsServiceFactory loggerFactory)
        {
            _usersDbContext = usersDbContext;
            _logger = loggerFactory.CreateAnalyticsService<Handler>();
        }
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var dbSearchProfile = await _usersDbContext.SearchProfiles.SingleAsync(o => o.Id == request.Id, cancellationToken);
            dbSearchProfile.Name = request.Name ?? dbSearchProfile.Name;
            dbSearchProfile.JobTitles = request.JobTitles ?? dbSearchProfile.JobTitles;
            dbSearchProfile.Regions = request.Regions ?? dbSearchProfile.Regions;
            dbSearchProfile.Modified = DateTime.UtcNow;

            await _usersDbContext.SaveChangesAsync(cancellationToken);

            await _logger.LogAuditLogEvent(AuditLogEvent.Update, request.User);

            return Unit.Value;
        }
    }

    [SwaggerSchema(Title = "UpdateSearchProfileResponse")]
    public record SearchProfile(Guid Id);
}