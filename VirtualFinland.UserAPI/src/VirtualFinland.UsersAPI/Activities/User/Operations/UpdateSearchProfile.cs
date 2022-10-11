using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class UpdateSearchProfile
{
    [SwaggerSchema(Title = "UpdateSearchProfile")]
    public class UpdateSearchProfileCommand : IRequest
    {
        public Guid Id { get; }
        public List<string> JobTitles { get; }
        public List<string> Regions { get; }
        
        public string? Name { get; }
        [SwaggerIgnore]
        public string? ClaimsUserId { get; set; }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public UpdateSearchProfileCommand(Guid id, List<string> jobTitles, List<string> regions, string name)
        {
            this.Id = id;
            this.JobTitles = jobTitles;
            this.Regions = regions;
            this.Name = name;
        }

        public void SetAuth(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsIssuer = claimsIssuer;
            this.ClaimsUserId = claimsUserId;
        }
    }

    public class Handler : IRequestHandler<UpdateSearchProfileCommand>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }
        public async Task<Unit> Handle(UpdateSearchProfileCommand request, CancellationToken cancellationToken)
        {
            var dbSearchProfile = await _usersDbContext.SearchProfiles.SingleAsync(o => o.Id == request.Id, cancellationToken);
            dbSearchProfile.Name = request.Name ?? dbSearchProfile.Name;
            dbSearchProfile.JobTitles = request.JobTitles ?? dbSearchProfile.JobTitles;
            dbSearchProfile.Regions = request.Regions ?? dbSearchProfile.Regions;
            dbSearchProfile.Modified = DateTime.UtcNow;

            await _usersDbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Search Profile updated: {RequestId}", request.Id);
            
            return Unit.Value;
        }
    }

    public record SearchProfile(Guid Id);
}