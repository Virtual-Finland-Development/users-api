using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Extensions;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class CreateSearchProfile
{
    [SwaggerSchema(Title = "CreateSearchProfileRequest")]
    public class Command : AuthenticatedRequest<SearchProfile>
    {
        public List<string> JobTitles { get; }
        public List<string> Regions { get; }

        public string? Name { get; }

        public Command(List<string> jobTitles, List<string> regions, string? name)
        {
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

    public class Handler : IRequestHandler<Command, SearchProfile>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly AnalyticsService<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, AnalyticsServiceFactory loggerFactory)
        {
            _usersDbContext = usersDbContext;
            _logger = loggerFactory.CreateAnalyticsService<Handler>();
        }

        public async Task<SearchProfile> Handle(Command request, CancellationToken cancellationToken)
        {
            var dbUser = await _usersDbContext.Persons.SingleAsync(o => o.Id == request.User.PersonId, cancellationToken: cancellationToken);

            var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new Models.UsersDatabase.SearchProfile()
            {
                Name = request.Name ?? request.JobTitles.FirstOrDefault(),
                PersonId = dbUser.Id,
                JobTitles = request.JobTitles,
                Regions = request.Regions,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }, cancellationToken);

            await _usersDbContext.SaveChangesAsync(cancellationToken);

            await _logger.LogAuditLogEvent(AuditLogEvent.Update, request.User);

            return new SearchProfile(dbNewSearchProfile.Entity.Id);
        }
    }
    [SwaggerSchema(Title = "CreateSearchProfileResponse")]
    public record SearchProfile(Guid Id);
}
