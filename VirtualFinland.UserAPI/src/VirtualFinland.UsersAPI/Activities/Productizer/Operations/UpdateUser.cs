using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Models.Repositories;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations;

public static class UpdateUser
{
    [SwaggerSchema(Title = "UpdateUserRequest")]
    public class Command : IRequest<User>
    {
        public string? FirstName { get; }
        public string? LastName { get; }
        
        public string? Address { get; }
        
        public bool? JobsDataConsent { get; }
        
        public bool? ImmigrationDataConsent { get; }
        
        public string? CountryOfBirthCode { get; }

        public string? NativeLanguageCode { get; }

        public string? OccupationCode { get; }

        public string? CitizenshipCode { get; }

        public List<string>? JobTitles { get; }
        public List<string>? Regions { get; }
        
        public string? Gender { get; }
        
        public DateTime? DateOfBirth { get; }
        
        [SwaggerIgnore]
        public Guid? UserId { get; private set; }
        
        [SwaggerIgnore]
        public string? Authorization { get; private set; }

        public Command(string? firstName,
            string? lastName,
            string? address,
            bool? jobsDataConsent,
            bool? immigrationDataConsent,
            string? countryOfBirthCode,
            string? nativeLanguageCode,
            string? occupationCode,
            string? citizenshipCode,
            List<string>? jobTitles,
            List<string>? regions,
            string? gender,
            DateTime? dateOfBirth)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Address = address;
            this.JobsDataConsent = jobsDataConsent;
            this.ImmigrationDataConsent = immigrationDataConsent;
            this.CountryOfBirthCode = countryOfBirthCode;
            this.NativeLanguageCode = nativeLanguageCode;
            this.OccupationCode = occupationCode;
            this.CitizenshipCode = citizenshipCode;
            this.JobTitles = jobTitles;
            this.Regions = regions;
            this.Gender = gender;
            this.DateOfBirth = dateOfBirth;
        }

        public void SetAuth(Guid? userDbId)
        {
            this.UserId = userDbId;
        }

        public void SetRequestAuthorization(string? authorization)
        {
            this.Authorization = authorization;
        }
    }
    
    public class CommandValidator : AbstractValidator<Command> 
    {
        public CommandValidator()
        {
            RuleFor(command => command.UserId).NotNull().NotEmpty();
            RuleFor(command => command.FirstName).MaximumLength(255);
            RuleFor(command => command.LastName).MaximumLength(255);
            RuleFor(command => command.Address).MaximumLength(512);
            RuleFor(command => command.CitizenshipCode).MaximumLength(10);
            RuleFor(command => command.OccupationCode).MaximumLength(10);
            RuleFor(command => command.NativeLanguageCode).MaximumLength(10);
            RuleFor(command => command.CountryOfBirthCode).MaximumLength(10);
            RuleFor(command => command.Gender).MaximumLength(10);
        }
    }
    

    public class Handler : IRequestHandler<Command, User>
        {
            private readonly UsersDbContext _usersDbContext;
            private readonly ILogger<Handler> _logger;
            private readonly ILanguageRepository _languageRepository;
            private readonly ICountriesRepository _countriesRepository;
            private readonly IOccupationsRepository _occupationsRepository;
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly IConfiguration _configuration;

            public Handler(UsersDbContext usersDbContext,
                ILogger<Handler> logger,
                ILanguageRepository languageRepository,
                ICountriesRepository countriesRepository,
                IOccupationsRepository occupationsRepository,
                IHttpClientFactory httpClientFactory,
                IConfiguration configuration)
            {
                _usersDbContext = usersDbContext;
                _logger = logger;
                _languageRepository = languageRepository;
                _countriesRepository = countriesRepository;
                _occupationsRepository = occupationsRepository;
                _httpClientFactory = httpClientFactory;
                _configuration = configuration;
            }

            public async Task<User> Handle(Command request, CancellationToken cancellationToken)
            {
                await ValidateToken(request);

                var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == request.UserId, cancellationToken: cancellationToken);
                
                await VerifyUserUpdate(dbUser, request);
                
                var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);
                dbUserDefaultSearchProfile = await VerifyUserSearchProfile(dbUserDefaultSearchProfile, dbUser, request, cancellationToken);

                await _usersDbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("User data updated for user: {DbUserId}", dbUser.Id);

                return new User(dbUser.Id,
                    dbUser.FirstName,
                    dbUser.LastName,
                    dbUser.Address,
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
                    dbUser.DateOfBirth?.ToDateTime(TimeOnly.MinValue));
            }
            
            private async Task ValidateToken(Command command)
            {
                try
                {
                    var token = command.Authorization.Replace("Bearer ", string.Empty) ?? string.Empty;
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var canReadToken = tokenHandler.CanReadToken(token);
                    var issuer = canReadToken ? tokenHandler.ReadJwtToken(token).Issuer : string.Empty;
                    
                    HttpClient httpClient = _httpClientFactory.CreateClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                    httpClient.DefaultRequestHeaders.Add(Constants.Headers.XAuthorizationContext, Constants.Web.AuthGwApplicationContext);
                    httpClient.DefaultRequestHeaders.Add(Constants.Headers.XAuthorizationProvider, issuer);
                    using HttpResponseMessage response = await httpClient.PostAsync(_configuration["AuthGW:AuthorizeURL"], null);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    throw new NotAuthorizedException(e.Message);
                }
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
                dbUser.Address = request.Address ?? dbUser.Address;
                dbUser.Modified = DateTime.UtcNow;
                dbUser.ImmigrationDataConsent = request.ImmigrationDataConsent ?? dbUser.ImmigrationDataConsent;
                dbUser.JobsDataConsent = request.JobsDataConsent ?? dbUser.JobsDataConsent;
                dbUser.CitizenshipCode = request.CitizenshipCode ?? dbUser.CitizenshipCode;
                dbUser.NativeLanguageCode = request.NativeLanguageCode ?? dbUser.NativeLanguageCode; 
                dbUser.OccupationCode = request.OccupationCode ?? dbUser.OccupationCode;
                dbUser.CountryOfBirthCode = request.CountryOfBirthCode ?? dbUser.CountryOfBirthCode;
                dbUser.Gender = request.Gender ?? dbUser.Gender;
                dbUser.DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.GetValueOrDefault()) : dbUser.DateOfBirth;

            }

            private async Task<List<ValidationErrorDetail>> ValidateOccupationCodesLogic(Command request)
            {
                var validationErrors = new List<ValidationErrorDetail>();

                if(!string.IsNullOrEmpty(request.OccupationCode))
                {
                    var occupations = await _occupationsRepository.GetAllOccupations() ?? new List<OccupationRoot.Occupation>();
                    if (!occupations.Any(o => o.Id == request.OccupationCode))
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
                else
                {
                    dbUserDefaultSearchProfile.Name = dbUserDefaultSearchProfile.Name;
                    dbUserDefaultSearchProfile.JobTitles = request.JobTitles ?? dbUserDefaultSearchProfile.JobTitles;
                    dbUserDefaultSearchProfile.Regions = request.Regions ?? dbUserDefaultSearchProfile.Regions;
                    dbUserDefaultSearchProfile.IsDefault = true;
                    dbUserDefaultSearchProfile.Modified = DateTime.UtcNow;

                    return dbUserDefaultSearchProfile;
                }
            }
        }
    [SwaggerSchema(Title = "UpdateUserResponse")]
    public record User(Guid Id,
        string? FirstName,
        string? LastName,
        string? Address,
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
        string? Gender,
        DateTime? DateOfBirth);
}