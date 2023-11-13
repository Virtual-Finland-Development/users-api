using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.User;

public static class GetUserProfile
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
                .Include(p => p.Occupations)
                .Include(p => p.WorkPreferences)
                .Include(p => p.AdditionalInformation).ThenInclude(ai => ai!.Address)
                .SingleAsync(p => p.Id == request.User.PersonId, cancellationToken);

            // TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault && o.PersonId == dbUser.Id, cancellationToken);

            List<UserResponseOccupation>? occupations = null;
            if (dbUser.Occupations is not null)
            {
                occupations = dbUser.Occupations.Select(x =>
                    new UserResponseOccupation
                    (
                        x.EscoUri,
                        x.EscoCode,
                        x.WorkMonths
                    )).ToList();
            }

            UserResponseWorkPreferences? workPreferences = null;
            if (dbUser.WorkPreferences is not null)
            {
                workPreferences = new UserResponseWorkPreferences(
                    dbUser.WorkPreferences?.PreferredRegionCode?.ToList(),
                    dbUser.WorkPreferences?.PreferredMunicipalityCode?.ToList(),
                    dbUser.WorkPreferences?.EmploymentTypeCode,
                    dbUser.WorkPreferences?.WorkingTimeCode,
                    dbUser.WorkPreferences?.WorkingLanguageEnum?.ToList().Aggregate((a, b) => a + "," + b), // Definition needs a string, transform to comma separated list
                    dbUser.WorkPreferences?.NaceCode,
                    dbUser.WorkPreferences?.Created,
                    dbUser.WorkPreferences?.Modified
                );
            }

            await _logger.LogAuditLogEvent(AuditLogEvent.Read, request.User);

            return new User
            {
                Id = dbUser.Id,
                FirstName = dbUser.GivenName,
                LastName = dbUser.LastName,
                Address = new Address(
                        dbUser.AdditionalInformation?.Address?.StreetAddress,
                        dbUser.AdditionalInformation?.Address?.ZipCode,
                        dbUser.AdditionalInformation?.Address?.City,
                        dbUser.AdditionalInformation?.Address?.Country
                    ),
                JobTitles = dbUserDefaultSearchProfile?.JobTitles ?? new List<string>(),
                Regions = dbUserDefaultSearchProfile?.Regions ?? new List<string>(),
                Created = dbUser.Created,
                Modified = dbUser.Modified,
                CountryOfBirthCode = dbUser.AdditionalInformation?.CountryOfBirthCode,
                NativeLanguageCode = dbUser.AdditionalInformation?.NativeLanguageCode,
                OccupationCode = dbUser.AdditionalInformation?.OccupationCode,
                CitizenshipCode = dbUser.AdditionalInformation?.CitizenshipCode,
                Gender = dbUser.AdditionalInformation?.Gender,
                DateOfBirth = dbUser.AdditionalInformation?.DateOfBirth,
                Occupations = occupations,
                WorkPreferences = workPreferences
            };
        }
    }

    [SwaggerSchema(Title = "UserResponse")]
    public record User
    {
        public Guid Id { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public Address? Address { get; init; }
        public List<string>? JobTitles { get; init; }
        public List<string>? Regions { get; init; }
        public DateTime Created { get; init; }
        public DateTime Modified { get; init; }
        public string? CountryOfBirthCode { get; init; }
        public string? NativeLanguageCode { get; init; }
        public string? OccupationCode { get; init; }
        public string? CitizenshipCode { get; init; }
        public string? Gender { get; init; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? DateOfBirth { get; init; }

        public List<UserResponseOccupation>? Occupations { get; init; }
        public UserResponseWorkPreferences? WorkPreferences { get; init; }
    }

    [SwaggerSchema(Title = "UserResponseWorkPreferences")]
    public record UserResponseWorkPreferences
    (
        List<string>? PreferredRegion,
        List<string>? PreferredMunicipality,
        string? TypeOfEmployment,
        string? WorkingTime,
        string? WorkingLanguage,
        string? NaceCode,
        DateTime? Created,
        DateTime? Modified
    );

    [SwaggerSchema(Title = "UserResponseOccupations")]
    public record UserResponseOccupation(
        string? EscoIdentifier,
        string? EscoCode,
        int? WorkExperience
    );
}
