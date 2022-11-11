using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class GetSearchProfile
{
    [SwaggerSchema(Title = "SearchProfileRequest")]
    public class Query : IRequest<SearchProfile>
    {
        [SwaggerIgnore]
        public Guid? UserId { get; }

        
        public Guid ProfileId { get; }

        public Query(Guid? userId, Guid profileId)
        {
            this.UserId = userId;
            this.ProfileId = profileId;
        }
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(query => query.UserId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Query, SearchProfile>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(IUserRepository userRepository, ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<SearchProfile> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await _userRepository.GetUser(request.UserId, cancellationToken);
            var userSearchProfile = await _userRepository.GetSearchProfile(request.ProfileId, cancellationToken);
            if (userSearchProfile is null)
            {
                _logger.LogInformation("Failed to retrieve search profile: {RequestProfileId}", request.ProfileId);
                throw new NotFoundException($"Specified search profile not found by ID: {request.ProfileId}");
            }
            
            _logger.LogDebug("Search Profile Retrieved: {SearchProfileId}", userSearchProfile.Id);

            return new SearchProfile(userSearchProfile.Id, userSearchProfile.JobTitles, userSearchProfile.Name, userSearchProfile.Regions, userSearchProfile.Created, userSearchProfile.Modified);
        }
    }
    
    [SwaggerSchema(Title = "SearchProfileResponse")]
    public record SearchProfile(Guid Id, List<string>? JobTitles, string? Name, List<string>? Regions, DateTime Created, DateTime Modified);
}

