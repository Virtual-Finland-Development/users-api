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
        public String? JobTitles { get; }
        public String? Municipality { get; }
        public string? Regions { get; }
        
        public string? Name { get; }
        [SwaggerIgnore]
        public string? ClaimsUserId { get; set; }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public UpdateSearchProfileCommand(Guid id, string jobTitles, string municipality, string regions, string name)
        {
            this.Id = id;
            this.JobTitles = jobTitles;
            this.Municipality = municipality;
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
        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }
        public async Task<Unit> Handle(UpdateSearchProfileCommand request, CancellationToken cancellationToken)
        {
            var dbSearchProfile = await _usersDbContext.SearchProfiles.SingleAsync(o => o.Id == request.Id, cancellationToken);
            dbSearchProfile.Name = request.Name ?? dbSearchProfile.Name;
            dbSearchProfile.JobTitles = request.JobTitles ?? dbSearchProfile.JobTitles;
            dbSearchProfile.Municipality = request.Municipality ?? dbSearchProfile.Municipality;
            dbSearchProfile.Regions = request.Regions ?? dbSearchProfile.Regions;

            await _usersDbContext.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }

    public record SearchProfile(Guid Id);
}