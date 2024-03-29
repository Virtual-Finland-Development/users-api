using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class GetUser
{
    [SwaggerSchema(Title = "UserRequest")]
    public class Query : AuthenticatedRequest<User>
    {
        public Query(RequestAuthenticatedUser RequestAuthenticatedUser) : base(RequestAuthenticatedUser)
        {
        }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(query => query.User.PersonId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Query, User>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly AnalyticsLogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, AnalyticsLoggerFactory loggerFactory)
        {
            _usersDbContext = usersDbContext;
            _logger = loggerFactory.CreateAnalyticsLogger<Handler>();
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await _usersDbContext.Persons
                .Include(u => u.Occupations)
                .Include(u => u.WorkPreferences)
                .Include(u => u.AdditionalInformation).ThenInclude(ai => ai!.Address)
                .SingleAsync(o => o.Id == request.User.PersonId, cancellationToken);

            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault && o.PersonId == dbUser.Id, cancellationToken);

            List<UserResponseOccupation>? occupations = null;
            if (dbUser.Occupations is not null)
            {
                occupations = dbUser.Occupations.Select(x =>
                    new UserResponseOccupation
                    (
                        x.Id,
                        x.EscoUri,
                        x.EscoCode,
                        x.WorkMonths
                    )).ToList();
            }

            UserResponseWorkPreferences? workPreferences = null;
            if (dbUser.WorkPreferences is not null)
            {
                workPreferences = new UserResponseWorkPreferences(
                    dbUser.WorkPreferences?.Id,
                    dbUser.WorkPreferences?.PreferredRegionCode?.ToList(),
                    dbUser.WorkPreferences?.PreferredMunicipalityCode?.ToList(),
                    dbUser.WorkPreferences?.EmploymentTypeCode,
                    dbUser.WorkPreferences?.WorkingTimeCode,
                    dbUser.WorkPreferences?.WorkingLanguageEnum?.ToList(),
                    dbUser.WorkPreferences?.NaceCode,
                    dbUser.WorkPreferences?.Created,
                    dbUser.WorkPreferences?.Modified
                );
            }

            await _logger.LogAuditLogEvent(AuditLogEvent.Read, request.User);

            return new User(
                dbUser.Id,
                dbUser.GivenName,
                dbUser.LastName,
                new Address(
                        dbUser.AdditionalInformation?.Address?.StreetAddress,
                        dbUser.AdditionalInformation?.Address?.ZipCode,
                        dbUser.AdditionalInformation?.Address?.City,
                        dbUser.AdditionalInformation?.Address?.Country
                 ),
                dbUserDefaultSearchProfile?.JobTitles,
                dbUserDefaultSearchProfile?.Regions,
                dbUser.Created,
                dbUser.Modified,
                dbUser.AdditionalInformation?.CountryOfBirthCode,
                dbUser.AdditionalInformation?.NativeLanguageCode,
                dbUser.AdditionalInformation?.OccupationCode,
                dbUser.AdditionalInformation?.CitizenshipCode,
                dbUser.AdditionalInformation?.Gender,
                dbUser.AdditionalInformation?.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                occupations,
                workPreferences
            );
        }
    }

    [SwaggerSchema(Title = "UserResponse")]
    public record User(
        Guid Id,
        string? FirstName,
        string? LastName,
        Address? Address,
        List<string>? JobTitles,
        List<string>? Regions,
        DateTime Created,
        DateTime Modified,
        string? CountryOfBirthCode,
        string? NativeLanguageCode,
        string? OccupationCode,
        string? CitizenshipCode,
        string? Gender,
        DateTime? DateOfBirth,
        List<UserResponseOccupation>? Occupations,
        UserResponseWorkPreferences? WorkPreferences
    );

    [SwaggerSchema(Title = "UserResponseWorkPreferences")]
    public record UserResponseWorkPreferences
    (
        Guid? Id,
        List<string>? PreferredRegionEnum,
        List<string>? PreferredMunicipalityEnum,
        string? EmploymentTypeCode,
        string? WorkingTimeEnum,
        List<string>? WorkingLanguageEnum,
        string? NaceCode,
        DateTime? Created,
        DateTime? Modified
    );

    [SwaggerSchema(Title = "UserResponseOccupations")]
    public record UserResponseOccupation(
        Guid Id,
        string? EscoUri,
        string? EscoCode,
        int? WorkMonths
    );
}
