using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

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
        public string? ClaimsUserId { get; set; }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public Command(Guid id, List<string> jobTitles, List<string> regions, string name)
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

    public class Handler : IRequestHandler<Command>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await VerifyAuthenticatedUser(request, cancellationToken);
            var dbSearchProfile = await _usersDbContext.SearchProfiles.SingleAsync(o => o.Id == request.Id, cancellationToken);
            dbSearchProfile.Name = request.Name ?? dbSearchProfile.Name;
            dbSearchProfile.JobTitles = request.JobTitles ?? dbSearchProfile.JobTitles;
            dbSearchProfile.Regions = request.Regions ?? dbSearchProfile.Regions;
            dbSearchProfile.Modified = DateTime.UtcNow;

            await _usersDbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Search Profile updated: {RequestId}", request.Id);
            
            return Unit.Value;
        }
        
        private async Task VerifyAuthenticatedUser(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);
                await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", request.ClaimsUserId, request.ClaimsIssuer);
                throw new NotAuthorizedException("User could not be identified as a valid user.", e);
            }
        }
    }

    [SwaggerSchema(Title = "UpdateSearchProfileResponse")]
    public record SearchProfile(Guid Id);
}