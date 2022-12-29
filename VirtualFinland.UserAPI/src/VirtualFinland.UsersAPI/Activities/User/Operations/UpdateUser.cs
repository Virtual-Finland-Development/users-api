using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Models.Repositories;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class UpdateUser
{
    [SwaggerSchema(Title = "UpdateUserRequest")]
    public class Command : IRequest<User>
    {
        public string? FirstName { get; }
        public string? LastName { get; }
        public Address? Address { get; }
        public bool? JobsDataConsent { get; }
        public bool? ImmigrationDataConsent { get; }
        public string? CountryOfBirthCode { get; }
        public string? NativeLanguageCode { get; }
        public string? OccupationCode { get; }
        public string? CitizenshipCode { get; }
        public List<string>? JobTitles { get; }
        public List<string>? Regions { get; }
        public Gender? Gender { get; }
        public DateTime? DateOfBirth { get; }
        public List<Occupation>? Occupations { get; }
        public WorkPreferences? WorkPreferences { get; }

        [SwaggerIgnore]
        public Guid? UserId { get; private set; }

        public Command(
            string? firstName,
            string? lastName,
            Address? address,
            bool? jobsDataConsent,
            bool? immigrationDataConsent,
            string? countryOfBirthCode,
            string? nativeLanguageCode,
            string? occupationCode,
            string? citizenshipCode,
            List<string>? jobTitles,
            List<string>? regions,
            Gender? gender,
            DateTime? dateOfBirth,
            List<Occupation>? occupations,
            WorkPreferences? workPreferences
        )
        {
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            JobsDataConsent = jobsDataConsent;
            ImmigrationDataConsent = immigrationDataConsent;
            CountryOfBirthCode = countryOfBirthCode;
            NativeLanguageCode = nativeLanguageCode;
            OccupationCode = occupationCode;
            CitizenshipCode = citizenshipCode;
            JobTitles = jobTitles;
            Regions = regions;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            Occupations = occupations;
            WorkPreferences = workPreferences;
        }

        public void SetAuth(Guid? userDbId)
        {
            UserId = userDbId;
        }
    }

    private sealed class WorkPreferencesValidator : AbstractValidator<WorkPreferences>
    {
        public WorkPreferencesValidator()
        {
            RuleForEach(wp => wp.PreferredMunicipalityEnum).IsInEnum();
        }
    }
    
    public class AddressValidator : AbstractValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(address => address.StreetAddress).MaximumLength(255);
            RuleFor(address => address.City).MaximumLength(255);
            RuleFor(address => address.Country).MaximumLength(255);
            RuleFor(address => address.ZipCode).MaximumLength(5);
        }
    }

    public class CommandValidator : AbstractValidator<Command> 
    {
        public CommandValidator()
        {
            RuleFor(command => command.UserId).NotNull().NotEmpty();
            RuleFor(command => command.FirstName).MaximumLength(255);
            RuleFor(command => command.LastName).MaximumLength(255);
            RuleFor(command => command.Address).SetValidator(new AddressValidator()!);
            RuleFor(command => command.CitizenshipCode).MaximumLength(10);
            RuleFor(command => command.OccupationCode).MaximumLength(10);
            RuleFor(command => command.NativeLanguageCode).MaximumLength(10);
            RuleFor(command => command.CountryOfBirthCode).MaximumLength(10);
            RuleFor(command => command.Gender).IsInEnum();
            RuleFor(command => command.WorkPreferences).SetValidator(new WorkPreferencesValidator()!);
        }
    }
    

    public class Handler : IRequestHandler<Command, User>
        {
            private readonly UsersDbContext _usersDbContext;
            private readonly ILogger<Handler> _logger;
            private readonly ILanguageRepository _languageRepository;
            private readonly ICountriesRepository _countriesRepository;
            private readonly IOccupationsFlatRepository _occupationsFlatRepository;

            public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger, ILanguageRepository languageRepository, ICountriesRepository countriesRepository, IOccupationsFlatRepository occupationsFlatRepository)
            {
                _usersDbContext = usersDbContext;
                _logger = logger;
                _languageRepository = languageRepository;
                _countriesRepository = countriesRepository;
                _occupationsFlatRepository = occupationsFlatRepository;
            }

            public async Task<User> Handle(Command request, CancellationToken cancellationToken)
            {
                var dbUser = await _usersDbContext.Users
                    .Include(u => u.WorkPreferences)
                    .Include(u => u.Occupations)
                    .SingleAsync(o => o.Id == request.UserId, cancellationToken: cancellationToken);
                
                await VerifyUserUpdate(dbUser, request);
                
                var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);
                dbUserDefaultSearchProfile = await VerifyUserSearchProfile(dbUserDefaultSearchProfile, dbUser, request, cancellationToken);

                await _usersDbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("User data updated for user: {DbUserId}", dbUser.Id);

                return new User(
                    dbUser.Id,
                    dbUser.FirstName,
                    dbUser.LastName,
                    new Address(
                        dbUser.StreetAddress,
                        dbUser.ZipCode, // TODO: Return actual data
                        dbUser.City,
                        dbUser.Country
                    ),
                    dbUserDefaultSearchProfile.JobTitles,
                    dbUserDefaultSearchProfile.Regions,
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
                    (dbUser.Occupations ?? new List<Occupation>()).ToList(),
                    dbUser.WorkPreferences
                );
            }

            private async Task VerifyUserUpdate(Models.UsersDatabase.User dbUser, Command request)
            {
                
                var validationErrors = new List<ValidationErrorDetail>();
                validationErrors.AddRange(await ValidateCountryCodesLogic(request));
                validationErrors.AddRange(await ValidateLanguageCodesLogic(request));
                validationErrors.AddRange(await ValidateOccupationCodesLogic(request));

                if (validationErrors.Count > 0)
                {
                    throw new BadRequestException("One or more validation errors occurred.", validationErrors);
                }

                dbUser.FirstName = request.FirstName ?? dbUser.FirstName;
                dbUser.LastName = request.LastName ?? dbUser.LastName;
                dbUser.StreetAddress = request.Address?.StreetAddress ?? dbUser.StreetAddress;
                dbUser.ZipCode = request.Address?.ZipCode ?? dbUser.ZipCode;
                dbUser.City = request.Address?.City ?? dbUser.City;
                dbUser.Country = request.Address?.Country ?? dbUser.Country;
                dbUser.Modified = DateTime.UtcNow;
                dbUser.ImmigrationDataConsent = request.ImmigrationDataConsent ?? dbUser.ImmigrationDataConsent;
                dbUser.JobsDataConsent = request.JobsDataConsent ?? dbUser.JobsDataConsent;
                dbUser.CitizenshipCode = request.CitizenshipCode ?? dbUser.CitizenshipCode;
                dbUser.NativeLanguageCode = request.NativeLanguageCode ?? dbUser.NativeLanguageCode; 
                dbUser.OccupationCode = request.OccupationCode ?? dbUser.OccupationCode;
                dbUser.CountryOfBirthCode = request.CountryOfBirthCode ?? dbUser.CountryOfBirthCode;
                dbUser.Gender = request.Gender ?? dbUser.Gender;
                dbUser.DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.GetValueOrDefault()) : dbUser.DateOfBirth;
                dbUser.Occupations = GetUpdatedOccupations(dbUser.Occupations, request.Occupations);
                dbUser.WorkPreferences = request.WorkPreferences ?? dbUser.WorkPreferences;

            }

            private static ICollection<Occupation>? GetUpdatedOccupations(
                ICollection<Occupation>? dbUserOccupations, 
                List<Occupation>? requestOccupations)
            {
                // Update occupations if the id matches from request to one existing id
                // Otherwise create new occupation.
                // All old occupations will be detached from user but not deleted from database
                if (requestOccupations is { Count: > 0 })
                {
                    dbUserOccupations ??= new List<Occupation>();

                    // Loop through all the occupations in the request
                    foreach (var occupation in requestOccupations)
                    {
                        // Check if Id field is set
                        if (occupation.Id == Guid.Empty)
                        {
                            // Id not set, add new occupation
                            dbUserOccupations.Add(occupation);
                            continue;
                        }
                        
                        var existingOccupation = dbUserOccupations.FirstOrDefault(o => o.Id == occupation.Id);
                        
                        // TODO: Return some error about invalid guid ?
                        if(existingOccupation is null) continue;
                        
                        // else update property values
                        existingOccupation.Update(occupation);
                    }
                }
                else
                {
                    return new List<Occupation>();
                }

                return dbUserOccupations;
            }

            private async Task<List<ValidationErrorDetail>> ValidateOccupationCodesLogic(Command request)
            {
                var validationErrors = new List<ValidationErrorDetail>();

                if(!string.IsNullOrEmpty(request.OccupationCode))
                {
                    var occupations = await _occupationsFlatRepository.GetAllOccupationsFlat() ?? new List<OccupationFlatRoot.Occupation>();
                    if (!occupations.Any(o => o.Notation == request.OccupationCode))
                    {
                        validationErrors.Add(new ValidationErrorDetail(nameof(request.OccupationCode), $"{nameof(request.OccupationCode)} does not match any known occupation code."));
                    }    
                }
                
                return validationErrors;
            }

            private async Task<List<ValidationErrorDetail>>  ValidateLanguageCodesLogic(Command request)
            {
                var validationErrors = new List<ValidationErrorDetail>();

                if (!string.IsNullOrEmpty(request.NativeLanguageCode))
                {
                    var languages = await _languageRepository.GetAllLanguages();
                    if (!languages.Any(o => o.Id == request.NativeLanguageCode))
                    {
                        validationErrors.Add(new ValidationErrorDetail(nameof(request.NativeLanguageCode), $"{nameof(request.NativeLanguageCode)} does not match any known language code."));
                    }    
                }

                return validationErrors;
            }
            private async Task<List<ValidationErrorDetail>>  ValidateCountryCodesLogic(Command request)
            {
                var countries = new List<Country>();
                var validationErrors = new List<ValidationErrorDetail>();

                if (!string.IsNullOrEmpty(request.CitizenshipCode) || !string.IsNullOrEmpty(request.CountryOfBirthCode))
                {
                    countries = await _countriesRepository.GetAllCountries();
                    if (!string.IsNullOrEmpty(request.CitizenshipCode) && !countries.Any(o => o.IsoCode == request.CitizenshipCode?.ToUpper()))
                    {
                        validationErrors.Add(new ValidationErrorDetail(nameof(request.CitizenshipCode), $"{nameof(request.CitizenshipCode)} does not match any known ISO 3166 country code."));
                    }
                
                    if (!string.IsNullOrEmpty(request.CountryOfBirthCode) && !countries.Any(o => o.IsoCode == request.CountryOfBirthCode?.ToUpper()))
                    {
                        validationErrors.Add(new ValidationErrorDetail(nameof(request.CountryOfBirthCode), $"{nameof(request.CountryOfBirthCode)} does not match any known ISO 3166 country code."));
                    }
                }

                return validationErrors;
            }

            /// <summary>
            /// TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
            /// </summary>
            /// <param name="dbUserDefaultSearchProfile"></param>
            /// <param name="dbUser"></param>
            /// <param name="request"></param>
            /// <param name="cancellationToken"></param>
            private async Task<SearchProfile> VerifyUserSearchProfile(SearchProfile? dbUserDefaultSearchProfile, Models.UsersDatabase.User dbUser, Command request, CancellationToken cancellationToken)
            {
                if (dbUserDefaultSearchProfile is null)
                {
                    var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new SearchProfile()
                    {
                        Name = request.JobTitles?.FirstOrDefault(),
                        UserId = dbUser.Id,
                        JobTitles = request.JobTitles,
                        Regions = request.Regions,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        IsDefault = true
                    }, cancellationToken);

                    return dbNewSearchProfile.Entity;
                }
                
                dbUserDefaultSearchProfile.JobTitles = request.JobTitles ?? dbUserDefaultSearchProfile.JobTitles;
                dbUserDefaultSearchProfile.Regions = request.Regions ?? dbUserDefaultSearchProfile.Regions;
                dbUserDefaultSearchProfile.IsDefault = true;
                dbUserDefaultSearchProfile.Modified = DateTime.UtcNow;

                return dbUserDefaultSearchProfile;
            }
        }

    [SwaggerSchema(Title = "UpdateUserResponse")]
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
        List<Occupation>? Occupations,
        WorkPreferences? WorkPreferences
    );
}
