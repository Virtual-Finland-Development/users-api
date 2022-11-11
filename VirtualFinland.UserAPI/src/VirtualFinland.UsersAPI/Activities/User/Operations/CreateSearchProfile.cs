using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class CreateSearchProfile
{
    [SwaggerSchema(Title = "CreateSearchProfileRequest")]
    public class Command : IRequest<SearchProfile>
    {
        public List<string> JobTitles { get; }
        public List<string> Regions { get; }
        
        public string? Name { get; }
        
        [SwaggerIgnore]
        public Guid? UserId { get; private set; }

        public Command(List<string> jobTitles, List<string> regions, string? name)
        {
            this.JobTitles = jobTitles;
            this.Regions = regions;
            this.Name = name;
        }
        
        public void SetAuth(Guid? userDbId)
        {
            this.UserId = userDbId;
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.UserId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, SearchProfile>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(IUserRepository userRepository, ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<SearchProfile> Handle(Command request, CancellationToken cancellationToken)
        {
            var dbUser = await _userRepository.GetUser(request.UserId, cancellationToken);
            
            var dbNewSearchProfile = await _userRepository.AddSearchProfile(new Models.UsersDatabase.SearchProfile()
            {
                Name = request.Name ?? request.JobTitles.FirstOrDefault(),
                UserId = dbUser.Id,
                JobTitles = request.JobTitles,
                Regions = request.Regions,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }, cancellationToken);
            
            _logger.LogDebug("Search Profile Created: {SearchProfileId}", dbNewSearchProfile.Entity.Id);

            return new SearchProfile(dbNewSearchProfile.Entity.Id);
        }
    }
    [SwaggerSchema(Title = "CreateSearchProfileResponse")]
    public record SearchProfile(Guid Id);
}