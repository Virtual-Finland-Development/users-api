using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Models.Shared;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

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
                        x.Id,
                        x.EscoCode,
                        x.EscoUri,
                        x.NaceCode,
                        x.WorkMonths
                    )).ToList();
            }

            UserResponseWorkPreferences? workPreferences = null;
            if (dbUser.WorkPreferences is not null)
            {
                workPreferences = new UserResponseWorkPreferences(
                    dbUser.WorkPreferences?.Id,
                    dbUser.WorkPreferences?.PreferredRegionEnum,
                    dbUser.WorkPreferences?.PreferredMunicipalityEnum,
                    dbUser.WorkPreferences?.EmploymentTypeCode,
                    dbUser.WorkPreferences?.WorkingTimeEnum,
                    dbUser.WorkPreferences?.WorkingLanguageEnum,
                    dbUser.WorkPreferences?.Created,
                    dbUser.WorkPreferences?.Modified
                );
            }
            
            return new User(
                dbUser.Id,
                dbUser.FirstName,
                dbUser.LastName,
                new Address(
                    dbUser.StreetAddress,
                    dbUser.ZipCode,
                    dbUser.City,
                    dbUser.Country
                ),
                dbUserDefaultSearchProfile?.JobTitles,
                dbUserDefaultSearchProfile?.Regions,
                dbUser.Created,
                dbUser.Modified,
                dbUser.ImmigrationDataConsent,
                dbUser.JobsDataConsent,
                dbUser.CountryOfBirthCode,
                dbUser.NativeLanguageCode,
                dbUser.OccupationCode,
                dbUser.CitizenshipCode,
                dbUser.Gender,
                dbUser.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
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
        bool ImmigrationDataConsent,
        bool JobsDataConsent,
        string? CountryOfBirthCode,
        string? NativeLanguageCode,
        string? OccupationCode,
        string? CitizenshipCode,
        Gender? Gender,
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
        string? WorkingLanguageEnum,
        DateTime? Created,
        DateTime? Modified
    );

    [SwaggerSchema(Title = "UserResponseOccupations")]
    public record UserResponseOccupation(
        Guid Id,
        string? NaceCode,
        string? EscoUri,
        string? EscoCode,
        int? WorkMonths
    );


}
