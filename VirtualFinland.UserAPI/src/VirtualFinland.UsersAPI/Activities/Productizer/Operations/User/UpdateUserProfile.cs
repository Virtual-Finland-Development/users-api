using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Repositories;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Security.Models;
using Address = VirtualFinland.UserAPI.Models.Shared.Address;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.User;

public static class UpdateUserProfile
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

        public Command(string? firstName,
            string? lastName,
            Address? address,
            string? countryOfBirthCode,
            string? nativeLanguageCode,
            string? occupationCode,
            string? citizenshipCode,
            List<string>? jobTitles,
            List<string>? regions,
            string? gender,
            DateTime? dateOfBirth)
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
            RuleFor(command => command.User.PersonId).NotNull().NotEmpty();
            RuleFor(command => command.FirstName).MaximumLength(255);
            RuleFor(command => command.LastName).MaximumLength(255);
            RuleFor(command => command.Address).SetValidator(new AddressValidator()!);
            RuleFor(command => command.CitizenshipCode).MaximumLength(10);
            RuleFor(command => command.OccupationCode).MaximumLength(10);
            RuleFor(command => command.NativeLanguageCode).MaximumLength(10);
            RuleFor(command => command.CountryOfBirthCode).MaximumLength(10);
            RuleFor(command => command.Gender).IsInEnum();
        }
    }


    public class Handler : IRequestHandler<Command, User>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly AnalyticsService<Handler> _logger;
        private readonly ILanguageRepository _languageRepository;
        private readonly ICountriesRepository _countriesRepository;
        private readonly IOccupationsFlatRepository _occupationsFlatRepository;

        public Handler(UsersDbContext usersDbContext,
            AnalyticsServiceFactory loggerFactory,
            ILanguageRepository languageRepository,
            ICountriesRepository countriesRepository,
            IOccupationsFlatRepository occupationsFlatRepository)
        {
            _usersDbContext = usersDbContext;
            _logger = loggerFactory.CreateAnalyticsService<Handler>();
            _languageRepository = languageRepository;
            _countriesRepository = countriesRepository;
            _occupationsFlatRepository = occupationsFlatRepository;
        }

        public async Task<User> Handle(Command request, CancellationToken cancellationToken)
        {
            var dbUser = await _usersDbContext.Persons
                .Include(p => p.AdditionalInformation).ThenInclude(ai => ai!.Address)
                .SingleAsync(o => o.Id == request.User.PersonId, cancellationToken);

            await VerifyUserUpdate(dbUser, request);

            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault && o.PersonId == dbUser.Id, cancellationToken);
            dbUserDefaultSearchProfile = await VerifyUserSearchProfile(dbUserDefaultSearchProfile, dbUser, request, cancellationToken);

            await _usersDbContext.SaveChangesAsync(request.User, cancellationToken);

            await _logger.HandleAuditLogEvent(AuditLogEvent.Update, request.User);

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
                JobTitles = dbUserDefaultSearchProfile.JobTitles ?? new List<string>(),
                Regions = dbUserDefaultSearchProfile.Regions ?? new List<string>(),
                Created = dbUser.Created,
                Modified = dbUser.Modified,
                CountryOfBirthCode = dbUser.AdditionalInformation?.CountryOfBirthCode,
                NativeLanguageCode = dbUser.AdditionalInformation?.NativeLanguageCode,
                OccupationCode = dbUser.AdditionalInformation?.OccupationCode,
                CitizenshipCode = dbUser.AdditionalInformation?.CitizenshipCode,
                Gender = dbUser.AdditionalInformation?.Gender,
                DateOfBirth = dbUser.AdditionalInformation?.DateOfBirth
            };
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

            dbUser.GivenName = request.FirstName ?? dbUser.GivenName;
            dbUser.LastName = request.LastName ?? dbUser.LastName;

            dbUser.AdditionalInformation ??= new PersonAdditionalInformation();
            dbUser.AdditionalInformation.Address ??= new Models.UsersDatabase.Address();

            dbUser.AdditionalInformation.Address.StreetAddress = request.Address?.StreetAddress ?? dbUser.AdditionalInformation.Address.StreetAddress;
            dbUser.AdditionalInformation.Address.ZipCode = request.Address?.ZipCode ?? dbUser.AdditionalInformation.Address.ZipCode;
            dbUser.AdditionalInformation.Address.City = request.Address?.City ?? dbUser.AdditionalInformation.Address.City;
            dbUser.AdditionalInformation.Address.Country = request.Address?.Country ?? dbUser.AdditionalInformation.Address.Country;
            dbUser.AdditionalInformation.CitizenshipCode = request.CitizenshipCode ?? dbUser.AdditionalInformation.CitizenshipCode;
            dbUser.AdditionalInformation.NativeLanguageCode = request.NativeLanguageCode ?? dbUser.AdditionalInformation.NativeLanguageCode;
            dbUser.AdditionalInformation.OccupationCode = request.OccupationCode ?? dbUser.AdditionalInformation.OccupationCode;
            dbUser.AdditionalInformation.CountryOfBirthCode = request.CountryOfBirthCode ?? dbUser.AdditionalInformation.CountryOfBirthCode;
            dbUser.AdditionalInformation.Gender = request.Gender ?? dbUser.AdditionalInformation.Gender;
            dbUser.AdditionalInformation.DateOfBirth = request.DateOfBirth.HasValue
                ? DateOnly.FromDateTime(request.DateOfBirth.GetValueOrDefault())
                : dbUser.AdditionalInformation.DateOfBirth;
        }

        private async Task<List<ValidationErrorDetail>> ValidateOccupationCodesLogic(Command request)
        {
            var validationErrors = new List<ValidationErrorDetail>();

            if (!string.IsNullOrEmpty(request.OccupationCode))
            {
                var occupations = await _occupationsFlatRepository.GetAllOccupationsFlat() ?? new List<OccupationFlatRoot.Occupation>();
                if (!occupations.Any(o => o.Notation == request.OccupationCode))
                {
                    validationErrors.Add(new ValidationErrorDetail(nameof(request.OccupationCode), $"{nameof(request.OccupationCode)} does not match any known occupation code."));
                }
            }

            return validationErrors;
        }

        private async Task<List<ValidationErrorDetail>> ValidateLanguageCodesLogic(Command request)
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
        private async Task<List<ValidationErrorDetail>> ValidateCountryCodesLogic(Command request)
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
        private async Task<SearchProfile> VerifyUserSearchProfile(SearchProfile? dbUserDefaultSearchProfile, Person dbUser, Command request, CancellationToken cancellationToken)
        {
            if (dbUserDefaultSearchProfile is null)
            {
                var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new SearchProfile
                {
                    Name = request.JobTitles?.FirstOrDefault(),
                    PersonId = dbUser.Id,
                    JobTitles = request.JobTitles,
                    Regions = request.Regions,
                    IsDefault = true
                }, cancellationToken);

                return dbNewSearchProfile.Entity;
            }

            dbUserDefaultSearchProfile.JobTitles = request.JobTitles ?? dbUserDefaultSearchProfile.JobTitles;
            dbUserDefaultSearchProfile.Regions = request.Regions ?? dbUserDefaultSearchProfile.Regions;
            dbUserDefaultSearchProfile.IsDefault = true;

            return dbUserDefaultSearchProfile;
        }
    }

    [SwaggerSchema(Title = "UpdateUserResponse")]
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
    }
}
