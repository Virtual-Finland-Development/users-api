using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Models.Shared;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations;

public static class GetUser
{
    [SwaggerSchema(Title = "UserRequest")]
    public class Query : IRequest<User>
    {
        [SwaggerIgnore]
        public Guid? UserId { get; }

        public Query(Guid? userId)
        {
            this.UserId = userId;
        }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(query => query.UserId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Query, User>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {

            var dbUser = await _usersDbContext.Users
                .Include(u => u.Occupations)
                .Include(u => u.WorkPreferences)
                .SingleAsync(o => o.Id == request.UserId, cancellationToken);

            // TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);
            _logger.LogDebug("User data retrieved for user: {DbUserId}", dbUser.Id);

            List<UserResponseOccupation>? occupations = null;
            if (dbUser.Occupations is not null)
            {
                occupations = dbUser.Occupations.Select(x => 
                    new UserResponseOccupation
                    (
                        x.EscoUri,
                        x.EscoCode,
                        x.NaceCode,
                        x.WorkMonths
                    )).ToList();
            }

            UserResponseWorkPreferences? workPreferences = null;
            if (dbUser.WorkPreferences is not null)
            {
                workPreferences = new UserResponseWorkPreferences(
                    dbUser.WorkPreferences?.PreferredRegionEnum,
                    dbUser.WorkPreferences?.PreferredMunicipalityEnum,
                    dbUser.WorkPreferences?.EmploymentTypeCode,
                    dbUser.WorkPreferences?.WorkingTimeEnum,
                    dbUser.WorkPreferences?.WorkingLanguageEnum,
                    dbUser.WorkPreferences?.Created,
                    dbUser.WorkPreferences?.Modified
                );
            }
            
            return new User
            {
                Id = dbUser.Id,
                FirstName = dbUser.FirstName,
                LastName = dbUser.LastName,
                Address = new Address(
                        dbUser.StreetAddress,
                        dbUser.ZipCode,
                        dbUser.City,
                        dbUser.Country
                    ),
                JobTitles = dbUserDefaultSearchProfile?.JobTitles ?? new List<string>(),
                Regions = dbUserDefaultSearchProfile?.Regions ?? new List<string>(),
                Created = dbUser.Created,
                Modified = dbUser.Modified,
                CountryOfBirthCode = dbUser.CountryOfBirthCode,
                NativeLanguageCode = dbUser.NativeLanguageCode,
                OccupationCode = dbUser.OccupationCode,
                CitizenshipCode = dbUser.CitizenshipCode,
                Gender = dbUser.Gender,
                DateOfBirth = dbUser.DateOfBirth,
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
        public Gender? Gender { get; init; }
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
        DateTime? Created,
        DateTime? Modified
    );

    [SwaggerSchema(Title = "UserResponseOccupations")]
    public record UserResponseOccupation(
        string? EscoIdentifier,
        string? EscoCode,
        string? NaceCode,
        int? WorkExperience
    );
}
