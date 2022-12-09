
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
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
                DateOfBirth = dbUser.DateOfBirth
            };
        }
    }

    [SwaggerSchema(Title = "UpdateUserResponse")]
    public record User
    {
        public Guid Id { get; init; } = default!;
        public string? FirstName { get; init; } = default;
        public string? LastName { get; init; } = default!;
        public Address? Address { get; init; } = default!;
        public List<string>? JobTitles { get; init; } = default!;
        public List<string>? Regions { get; init; } = default!;
        public DateTime Created { get; init; } = default!;
        public DateTime Modified { get; init; } = default!;
        public string? CountryOfBirthCode { get; init; } = default!;
        public string? NativeLanguageCode { get; init; } = default!;
        public string? OccupationCode { get; init; } = default!;
        public string? CitizenshipCode { get; init; } = default!;
        public Gender? Gender { get; init; } = default!;
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? DateOfBirth { get; init; } = default!;
    }
}