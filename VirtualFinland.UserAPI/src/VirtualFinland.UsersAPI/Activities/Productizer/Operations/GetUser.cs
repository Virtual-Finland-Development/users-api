
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations;

public static class GetUser
{
    [SwaggerSchema(Title = "UserRequest")]
    public class Query : IRequest<User>
    {
        [SwaggerIgnore]
        public Guid? UserId { get; }
        [SwaggerIgnore]
        public string? Authorization { get; }

        public Query(Guid? userId, string? authorization)
        {
            this.UserId = userId;
            this.Authorization = authorization;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            await ValidateToken(request);

            var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == request.UserId, cancellationToken: cancellationToken);

            // TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);
            _logger.LogDebug("User data retrieved for user: {DbUserId}", dbUser.Id);
            
            return new User(dbUser.Id,
                dbUser.FirstName,
                dbUser.LastName,
                dbUser.Address,
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
                dbUser.DateOfBirth?.ToDateTime(TimeOnly.MinValue));
        }

        private async Task ValidateToken(Query request)
        {
            try
            {
                var token = request.Authorization.Replace("Bearer ", string.Empty) ?? string.Empty;
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
    }
    
    [SwaggerSchema(Title = "UserResponse")]
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