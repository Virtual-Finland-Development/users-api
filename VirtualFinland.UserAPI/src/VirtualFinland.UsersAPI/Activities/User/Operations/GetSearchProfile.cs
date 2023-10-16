using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class GetSearchProfile
{
    [SwaggerSchema(Title = "SearchProfileRequest")]
    public class Query : AuthenticatedRequest<SearchProfile>
    {
        public Guid ProfileId { get; }

        public Query(RequestAuthenticatedUser requestAuthenticatedUser, Guid profileId)
        {
            User = requestAuthenticatedUser;
            ProfileId = profileId;
        }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(query => query.User.PersonId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Query, SearchProfile>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly AnalyticsService _logger;

        public Handler(UsersDbContext usersDbContext, AnalyticsService logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<SearchProfile> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await _usersDbContext.Persons.SingleAsync(o => o.Id == request.User.PersonId, cancellationToken: cancellationToken);

            var userSearchProfile = await _usersDbContext.SearchProfiles.SingleOrDefaultAsync(o => o.PersonId == dbUser.Id && o.Id == request.ProfileId, cancellationToken);

            if (userSearchProfile is null)
            {
                _logger.LogInformation("Failed to retrieve search profile: {RequestProfileId}", request.ProfileId);
                throw new NotFoundException($"Specified search profile not found by ID: {request.ProfileId}");
            }

            _logger.LogAuditLogEvent(AuditLogEvent.Read, "SearchProfile", request.User);

            return new SearchProfile(userSearchProfile.Id, userSearchProfile.JobTitles, userSearchProfile.Name, userSearchProfile.Regions, userSearchProfile.Created, userSearchProfile.Modified);
        }
    }

    [SwaggerSchema(Title = "SearchProfileResponse")]
    public record SearchProfile(Guid Id, List<string>? JobTitles, string? Name, List<string>? Regions, DateTime Created, DateTime Modified);
}

