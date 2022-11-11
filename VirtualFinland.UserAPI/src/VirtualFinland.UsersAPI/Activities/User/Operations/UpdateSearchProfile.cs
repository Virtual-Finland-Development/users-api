using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class UpdateSearchProfile
{
    [SwaggerSchema(Title = "UpdateSearchProfileRequest")]
    public class Command : IRequest
    {
        public Guid Id { get; }
        public List<string>? JobTitles { get; }
        public List<string>? Regions { get; }
        
        public string? Name { get; }
        
        [SwaggerIgnore]
        public Guid? UserId { get; private set; }

        public Command(Guid id, List<string> jobTitles, List<string> regions, string name)
        {
            this.Id = id;
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

    public class Handler : IRequestHandler<Command>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(IUserRepository userRepository, ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var dbSearchProfile = await _userRepository.GetSearchProfile(request.Id, cancellationToken);
            dbSearchProfile.Name = request.Name ?? dbSearchProfile.Name;
            dbSearchProfile.JobTitles = request.JobTitles ?? dbSearchProfile.JobTitles;
            dbSearchProfile.Regions = request.Regions ?? dbSearchProfile.Regions;
            dbSearchProfile.Modified = DateTime.UtcNow;

            await _userRepository.UpdateSearchProfile(dbSearchProfile, cancellationToken);
            
            _logger.LogDebug("Search Profile updated: {RequestId}", request.Id);
            
            return Unit.Value;
        }
    }

    [SwaggerSchema(Title = "UpdateSearchProfileResponse")]
    public record SearchProfile(Guid Id);
}