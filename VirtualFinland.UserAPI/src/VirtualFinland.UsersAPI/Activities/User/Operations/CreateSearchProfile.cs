using MediatR;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class CreateSearchProfile
{
    [SwaggerSchema(Title = "CreateSearchProfile")]
    public class CreateSearchProfileCommand : IRequest<SearchProfile>
    {
        public List<string> JobTitles { get; }
        public List<string> Regions { get; }
        
        public string? Name { get; }
        
        [SwaggerIgnore]
        public string? ClaimsUserId { get; set;  }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public CreateSearchProfileCommand(List<string> jobTitles, List<string> regions, string? name)
        {
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

    public class Handler : IRequestHandler<CreateSearchProfileCommand, SearchProfile>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<SearchProfile> Handle(CreateSearchProfileCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUser = await GetAuthenticatedUser(request, cancellationToken);

            var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new Models.SearchProfile()
            {
                Name = request.Name ?? request.JobTitles.FirstOrDefault(),
                UserId = authenticatedUser.Id,
                JobTitles = request.JobTitles,
                Regions = request.Regions,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }, cancellationToken);

            await _usersDbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Search Profile Created: {SearchProfileId}", dbNewSearchProfile.Entity.Id);

            return new SearchProfile(dbNewSearchProfile.Entity.Id);
        }
        
        async private Task<Models.User> GetAuthenticatedUser(CreateSearchProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);
                return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", request.ClaimsUserId, request.ClaimsIssuer);
                throw new NotAuthorizedExpception("User could not be identified as a valid user.", e);
            }
        }
    }

    public record SearchProfile(Guid Id);
}