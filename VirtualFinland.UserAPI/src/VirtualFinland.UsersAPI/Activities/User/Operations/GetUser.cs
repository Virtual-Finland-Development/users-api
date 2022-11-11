using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers.Swagger;

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
        private readonly ILogger<Handler> _logger;
        private readonly IUserRepository _userRepository;

        public Handler(ILogger<Handler> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await _userRepository.GetUser(request.UserId, cancellationToken);
            // TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
            var dbUserDefaultSearchProfile = await _userRepository.GetUserDefaultSearchProfile(request.UserId, true, cancellationToken);
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