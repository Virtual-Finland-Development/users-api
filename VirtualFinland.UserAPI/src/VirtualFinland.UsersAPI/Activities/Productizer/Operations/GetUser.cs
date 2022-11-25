
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
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;

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

            var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == request.UserId, cancellationToken: cancellationToken);

            // TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
            var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);
            _logger.LogDebug("User data retrieved for user: {DbUserId}", dbUser.Id);
            
            return new User(dbUser.Id,
                dbUser.FirstName,
                dbUser.LastName,
                new Address(
                    dbUser.StreetAddress,
                    dbUser.ZipCode,
                    dbUser.City,
                    dbUser.Country
                ),
                dbUserDefaultSearchProfile?.JobTitles ?? new List<string>(),
                dbUserDefaultSearchProfile?.Regions ?? new List<string>(),
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
    }

    [SwaggerSchema(Title = "UserResponse")]
    public record User(Guid Id,
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
        DateTime? DateOfBirth);

}