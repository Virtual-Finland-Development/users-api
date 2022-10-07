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
        public String JobTitles { get; }
        public String Municipality { get; }
        public string? Regions { get; }
        
        public string? Name { get; }
        
        [SwaggerIgnore]
        public string? ClaimsUserId { get; set;  }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public CreateSearchProfileCommand(string jobTitles, string municipality, string? regions, string? name)
        {
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

    public class Handler : IRequestHandler<CreateSearchProfileCommand, SearchProfile>
    {
        private readonly UsersDbContext _usersDbContext;
        public Handler(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }
        public async Task<SearchProfile> Handle(CreateSearchProfileCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUser = await GetAuthenticatedUser(request, cancellationToken);

            var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new Models.SearchProfile()
            {
                Name = request.Name ?? request.JobTitles,
                UserId = authenticatedUser.Id,
                JobTitles = request.JobTitles,
                Municipality = request.Municipality,
                Regions = request.Regions
            }, cancellationToken);

            await _usersDbContext.SaveChangesAsync(cancellationToken);

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
                throw new NotAuthorizedExpception("User could not be identified as a valid user.", e);
            }
        }
    }

    public record SearchProfile(Guid Id);
}