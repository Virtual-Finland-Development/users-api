using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Extensions;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using Address = VirtualFinland.UserAPI.Models.Shared.Address;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class UpdateUser
{
    [SwaggerSchema(Title = "UpdateUserRequest")]
    public class Command : AuthenticatedRequest<User>
    {
        public string? FirstName { get; }
        public string? LastName { get; }
        public Address? Address { get; }
        public string? CountryOfBirthCode { get; }
        public string? NativeLanguageCode { get; }
        public string? OccupationCode { get; }
        public string? CitizenshipCode { get; }
        public List<string>? JobTitles { get; }
        public List<string>? Regions { get; }
        public string? Gender { get; }
        public DateTime? DateOfBirth { get; }
        public List<UpdateUserRequestOccupation>? Occupations { get; }
        public UpdateUserRequestWorkPreferences? WorkPreferences { get; }

        public Command(
            string? firstName,
            string? lastName,
            Address? address,
            string? countryOfBirthCode,
            string? nativeLanguageCode,
            string? occupationCode,
            string? citizenshipCode,
            List<string>? jobTitles,
            List<string>? regions,
            string? gender,
            DateTime? dateOfBirth,
            List<UpdateUserRequestOccupation>? occupations,
            UpdateUserRequestWorkPreferences? workPreferences
        )
        {
            FirstName = firstName;
            LastName = lastName;
            Address = address;
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
    }

    private sealed class WorkPreferencesValidator : AbstractValidator<UpdateUserRequestWorkPreferences>
    {
        public WorkPreferencesValidator()
        {
            RuleForEach(wp => wp.PreferredMunicipalityEnum)
                .Must(x => EnumUtilities.TryParseWithMemberName<Municipality>(x, out _));

            RuleForEach(wp => wp.PreferredRegionEnum)
                .Must(x => EnumUtilities.TryParseWithMemberName<Region>(x, out _));

            RuleFor(x => x.EmploymentTypeCode)
                .Must(x => EnumUtilities.TryParseWithMemberName<EmploymentType>(x!, out _))
                .When(x => !string.IsNullOrEmpty(x.EmploymentTypeCode));

            RuleFor(x => x.WorkingTimeEnum)
                .Must(x => EnumUtilities.TryParseWithMemberName<WorkingTime>(x!, out _))
                .When(x => !string.IsNullOrEmpty(x.WorkingTimeEnum));

            RuleForEach(wp => wp.WorkingLanguageEnum)
                .Must(x => EnumUtilities.TryParseWithMemberName<WorkingLanguage>(x, out _));
        }
    }

    private sealed class AddressValidator : AbstractValidator<Address>
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
            RuleFor(command => command.User.PersonId).NotNull().NotEmpty();
            RuleFor(command => command.FirstName).MaximumLength(255);
            RuleFor(command => command.LastName).MaximumLength(255);
            RuleFor(command => command.Address).SetValidator(new AddressValidator()!);
            RuleFor(command => command.CitizenshipCode).MaximumLength(10);
            RuleFor(command => command.OccupationCode).MaximumLength(10);
            RuleFor(command => command.NativeLanguageCode).MaximumLength(10);
            RuleFor(command => command.CountryOfBirthCode).MaximumLength(10);

            RuleFor(command => command.Gender)
                .Must(x => EnumUtilities.TryParseWithMemberName<Gender>(x!, out _))
                .When(x => !string.IsNullOrEmpty(x.Gender));

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
            var dbUser = await _usersDbContext.Persons
                .Include(u => u.WorkPreferences)
                .Include(u => u.Occupations)
                .Include(u => u.AdditionalInformation).ThenInclude(ai => ai!.Address)
                .SingleAsync(o => o.Id == request.User.PersonId, cancellationToken);

            await VerifyUserUpdate(dbUser, request);


            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault && o.PersonId == dbUser.Id, cancellationToken);
            dbUserDefaultSearchProfile = await VerifyUserSearchProfile(dbUserDefaultSearchProfile, dbUser, request, cancellationToken);

            dbUser.SetupPersonAuditEvents(_usersDbContext, request.User);

            await _usersDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("User data updated for user: {DbUserId}", dbUser.Id);

            List<UpdateUserResponseOccupation>? occupations = null;
            if (dbUser.Occupations is { Count: > 0 })
            {
                occupations = dbUser.Occupations.Select(o =>
                        new UpdateUserResponseOccupation(o.Id, o.EscoUri, o.EscoCode, o.WorkMonths))
                    .ToList();
            }

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
                dbUserDefaultSearchProfile.JobTitles,
                dbUserDefaultSearchProfile.Regions,
                dbUser.Created,
                dbUser.Modified,
                dbUser.AdditionalInformation?.CountryOfBirthCode,
                dbUser.AdditionalInformation?.NativeLanguageCode,
                dbUser.AdditionalInformation?.OccupationCode,
                dbUser.AdditionalInformation?.CitizenshipCode,
                dbUser.AdditionalInformation?.Gender,
                dbUser.AdditionalInformation?.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                occupations,
                dbUser.WorkPreferences is null
                    ? null
                    : new UpdateUserResponseWorkPreferences
                    (
                        dbUser.WorkPreferences?.Id,
                        dbUser.WorkPreferences?.PreferredRegionCode,
                        dbUser.WorkPreferences?.PreferredMunicipalityCode,
                        dbUser.WorkPreferences?.EmploymentTypeCode,
                        dbUser.WorkPreferences?.WorkingTimeCode,
                        dbUser.WorkPreferences?.WorkingLanguageEnum,
                        dbUser.WorkPreferences?.NaceCode,
                        dbUser.WorkPreferences?.Created,
                        dbUser.WorkPreferences?.Modified
                    )
            );
        }

        private async Task VerifyUserUpdate(Person dbUser, Command request)
        {

            var validationErrors = new List<ValidationErrorDetail>();
            validationErrors.AddRange(await ValidateCountryCodesLogic(request));
            validationErrors.AddRange(await ValidateLanguageCodesLogic(request));
            validationErrors.AddRange(await ValidateOccupationCodesLogic(request));

            if (validationErrors.Count > 0)
            {
                throw new BadRequestException("One or more validation errors occurred.", validationErrors);
            }

            dbUser.AdditionalInformation ??= new PersonAdditionalInformation();
            dbUser.AdditionalInformation.Address ??= new Models.UsersDatabase.Address();

            dbUser.GivenName = request.FirstName ?? dbUser.GivenName;
            dbUser.LastName = request.LastName ?? dbUser.LastName;
            dbUser.AdditionalInformation.Address.StreetAddress = request.Address?.StreetAddress ?? dbUser.AdditionalInformation.Address.StreetAddress;
            dbUser.AdditionalInformation.Address.ZipCode = request.Address?.ZipCode ?? dbUser.AdditionalInformation.Address.ZipCode;
            dbUser.AdditionalInformation.Address.City = request.Address?.City ?? dbUser.AdditionalInformation.Address.City;
            dbUser.AdditionalInformation.Address.Country = request.Address?.Country ?? dbUser.AdditionalInformation.Address.Country;
            dbUser.Modified = DateTime.UtcNow;
            dbUser.AdditionalInformation.CitizenshipCode = request.CitizenshipCode ?? dbUser.AdditionalInformation.CitizenshipCode;
            dbUser.AdditionalInformation.NativeLanguageCode = request.NativeLanguageCode ?? dbUser.AdditionalInformation.NativeLanguageCode;
            dbUser.AdditionalInformation.OccupationCode = request.OccupationCode ?? dbUser.AdditionalInformation.OccupationCode;
            dbUser.AdditionalInformation.CountryOfBirthCode = request.CountryOfBirthCode ?? dbUser.AdditionalInformation.CountryOfBirthCode;
            dbUser.AdditionalInformation.Gender = request.Gender ?? dbUser.AdditionalInformation.Gender;
            dbUser.AdditionalInformation.DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.GetValueOrDefault()) : dbUser.AdditionalInformation.DateOfBirth;
            dbUser.Occupations = GetUpdatedOccupations(dbUser.Occupations, request.Occupations);

            if (request.WorkPreferences is not null)
            {
                dbUser.WorkPreferences ??= new WorkPreferences();
                dbUser.WorkPreferences.PreferredMunicipalityCode = request.WorkPreferences.PreferredMunicipalityEnum ?? dbUser.WorkPreferences.PreferredMunicipalityCode;
                dbUser.WorkPreferences.PreferredRegionCode = request.WorkPreferences.PreferredRegionEnum ?? dbUser.WorkPreferences.PreferredRegionCode;
                dbUser.WorkPreferences.WorkingLanguageEnum = request.WorkPreferences.WorkingLanguageEnum ?? dbUser.WorkPreferences.WorkingLanguageEnum;
                dbUser.WorkPreferences.EmploymentTypeCode = request.WorkPreferences.EmploymentTypeCode ?? dbUser.WorkPreferences.EmploymentTypeCode;
                dbUser.WorkPreferences.WorkingTimeCode = request.WorkPreferences.WorkingTimeEnum ?? dbUser.WorkPreferences.WorkingTimeCode;
                dbUser.WorkPreferences.NaceCode = request.WorkPreferences.NaceCode ?? dbUser.WorkPreferences.NaceCode;
            }
        }

        private static ICollection<T> GetEnumsFromCollection<T>(ICollection<string> enums) where T : struct, Enum
        {
            var updatedRegions = new List<T>();

            if (enums is not { Count: > 0 })
                return updatedRegions;

            foreach (var enumString in enums)
            {
                var isRegion = Enum.TryParse<T>(enumString, out var region);
                if (isRegion)
                    updatedRegions.Add(region);
            }

            return updatedRegions;
        }

        /// <summary>
        /// Update occupations if id field in request matches existing id in database
        /// otherwise create new occupations
        /// all old occupations will be detached from user but not deleted from database
        /// </summary>
        /// <param name="dbUserOccupations"></param>
        /// <param name="requestOccupations"></param>
        /// <returns></returns>
        private static ICollection<Occupation> GetUpdatedOccupations(
            ICollection<Occupation>? dbUserOccupations,
            List<UpdateUserRequestOccupation>? requestOccupations)
        {
            dbUserOccupations ??= new List<Occupation>();

            if (requestOccupations is { Count: > 0 })
            {
                foreach (var occupation in requestOccupations)
                {
                    if (occupation.Id == Guid.Empty)
                    {
                        if (occupation is { EscoCode: null, EscoUri: null, WorkMonths: null }) continue;

                        dbUserOccupations.Add(new Occupation
                        {
                            WorkMonths = occupation.WorkMonths,
                            EscoUri = occupation.EscoUri,
                            EscoCode = occupation.EscoCode
                        });
                        continue;
                    }

                    var existingOccupation = dbUserOccupations.FirstOrDefault(o => o.Id == occupation.Id);

                    // TODO: Return some error about invalid guid ?
                    if (existingOccupation is null) continue;

                    if (occupation.Delete is true)
                    {
                        dbUserOccupations.Remove(existingOccupation);
                    }

                    existingOccupation.EscoCode = occupation.EscoCode ?? existingOccupation.EscoCode;
                    existingOccupation.EscoUri = occupation.EscoUri ?? existingOccupation.EscoUri;
                    existingOccupation.WorkMonths = occupation.WorkMonths ?? existingOccupation.WorkMonths;
                }
            }
            else if (requestOccupations is { Count: 0 })
            {
                return new List<Occupation>();
            }

            return dbUserOccupations;
        }

        private async Task<List<ValidationErrorDetail>> ValidateOccupationCodesLogic(Command request)
        {
            var validationErrors = new List<ValidationErrorDetail>();

            if (string.IsNullOrEmpty(request.OccupationCode)) return validationErrors;

            var occupations = await _occupationsFlatRepository.GetAllOccupationsFlat();
            if (occupations.All(o => o.Notation != request.OccupationCode))
            {
                validationErrors.Add(new ValidationErrorDetail(nameof(request.OccupationCode), $"{nameof(request.OccupationCode)} does not match any known occupation code."));
            }

            return validationErrors;
        }

        private async Task<List<ValidationErrorDetail>> ValidateLanguageCodesLogic(Command request)
        {
            var validationErrors = new List<ValidationErrorDetail>();

            if (string.IsNullOrEmpty(request.NativeLanguageCode)) return validationErrors;

            var languages = await _languageRepository.GetAllLanguages();
            if (languages.All(o => o.Id != request.NativeLanguageCode))
            {
                validationErrors.Add(new ValidationErrorDetail(nameof(request.NativeLanguageCode), $"{nameof(request.NativeLanguageCode)} does not match any known language code."));
            }

            return validationErrors;
        }

        private async Task<List<ValidationErrorDetail>> ValidateCountryCodesLogic(Command request)
        {
            var validationErrors = new List<ValidationErrorDetail>();

            if (string.IsNullOrEmpty(request.CitizenshipCode) && string.IsNullOrEmpty(request.CountryOfBirthCode))
            {
                return validationErrors;
            }

            var countries = await _countriesRepository.GetAllCountries();
            if (!string.IsNullOrEmpty(request.CitizenshipCode) && countries.All(o => o.IsoCode != request.CitizenshipCode?.ToUpper()))
            {
                validationErrors.Add(new ValidationErrorDetail(nameof(request.CitizenshipCode), $"{nameof(request.CitizenshipCode)} does not match any known ISO 3166 country code."));
            }

            if (!string.IsNullOrEmpty(request.CountryOfBirthCode) && countries.All(o => o.IsoCode != request.CountryOfBirthCode?.ToUpper()))
            {
                validationErrors.Add(new ValidationErrorDetail(nameof(request.CountryOfBirthCode), $"{nameof(request.CountryOfBirthCode)} does not match any known ISO 3166 country code."));
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
        private async Task<SearchProfile> VerifyUserSearchProfile(SearchProfile? dbUserDefaultSearchProfile, Person dbUser, Command request, CancellationToken cancellationToken)
        {
            if (dbUserDefaultSearchProfile is null)
            {
                var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new SearchProfile()
                {
                    Name = request.JobTitles?.FirstOrDefault(),
                    PersonId = dbUser.Id,
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
        string? CountryOfBirthCode,
        string? NativeLanguageCode,
        string? OccupationCode,
        string? CitizenshipCode,
        string? Gender,
        DateTime? DateOfBirth,
        List<UpdateUserResponseOccupation>? Occupations,
        UpdateUserResponseWorkPreferences? WorkPreferences
    );

    [SwaggerSchema(Title = "UpdateUserRequestWorkPreferences")]
    public record UpdateUserRequestWorkPreferences
    (
        List<string>? PreferredRegionEnum,
        List<string>? PreferredMunicipalityEnum,
        string? EmploymentTypeCode,
        string? WorkingTimeEnum,
        List<string>? WorkingLanguageEnum,
        string? NaceCode
    );

    [SwaggerSchema(Title = "UpdateUserResponseWorkPreferences")]
    public record UpdateUserResponseWorkPreferences
    (
        Guid? Id,
        ICollection<string>? PreferredRegionEnum,
        ICollection<string>? PreferredMunicipalityEnum,
        string? EmploymentTypeCode,
        string? WorkingTimeEnum,
        ICollection<string>? WorkingLanguageEnum,
        string? NaceCode,
        DateTime? Created,
        DateTime? Modified
    );

    [SwaggerSchema(Title = "UpdateUserResponseOccupation")]
    public record UpdateUserResponseOccupation(
        Guid Id,
        string? EscoUri,
        string? EscoCode,
        int? WorkMonths
    );

    [SwaggerSchema(Title = "UpdateUserRequestOccupation")]
    public record UpdateUserRequestOccupation(
        Guid Id,
        string? EscoUri,
        string? EscoCode,
        int? WorkMonths,
        bool? Delete = false
    );
}
