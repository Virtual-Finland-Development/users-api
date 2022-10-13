using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class UpdateUser
{
    [SwaggerSchema(Title = "UpdateUserRequest")]
    public class Command : IRequest<User>
    {
        public string? FirstName { get; }
        public string? LastName { get; }
        
        public string? Address { get; set; }

        public List<string>? JobTitles { get; }
        public List<string>? Regions { get; }
        
        [SwaggerIgnore]
        public string? ClaimsUserId { get; set; }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public Command(string? firstName, string? lastName, List<string> jobTitles, List<string> regions)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.JobTitles = jobTitles;
            this.Regions = regions;
        }

        public void SetAuth(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsIssuer = claimsIssuer;
            this.ClaimsUserId = claimsUserId;
        }
    }

    public class Handler : IRequestHandler<Command, User>
        {
            private readonly UsersDbContext _usersDbContext;
            private readonly ILogger<Handler> _logger;

            public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
            {
                _usersDbContext = usersDbContext;
                _logger = logger;
            }

            public async Task<User> Handle(Command request, CancellationToken cancellationToken)
            {
                var dbUser = await GetAuthenticatedUser(request, cancellationToken);

                dbUser.FirstName = request.FirstName ?? dbUser.FirstName;
                dbUser.LastName = request.LastName ?? dbUser.LastName;
                dbUser.Address = request.Address ?? dbUser.Address;
                dbUser.Modified = DateTime.UtcNow;
                
                // TODO - To be decided: This default search profile in the user API call can be possibly removed when requirement are more clear
                var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);

                if (dbUserDefaultSearchProfile is null)
                {
                    var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new Models.SearchProfile()
                    {
                        Name = request.JobTitles?.FirstOrDefault(),
                        UserId = dbUser.Id,
                        JobTitles = request.JobTitles,
                        Regions = request.Regions,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        IsDefault = true
                    }, cancellationToken);

                    dbUserDefaultSearchProfile = dbNewSearchProfile.Entity;
                }
                else
                {
                    dbUserDefaultSearchProfile.Name = dbUserDefaultSearchProfile.Name;
                    dbUserDefaultSearchProfile.JobTitles = request.JobTitles ?? dbUserDefaultSearchProfile.JobTitles;
                    dbUserDefaultSearchProfile.Regions = request.Regions ?? dbUserDefaultSearchProfile.Regions;
                    dbUserDefaultSearchProfile.IsDefault = true;
                    dbUserDefaultSearchProfile.Modified = DateTime.UtcNow;
                }

                await _usersDbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("User data updated for user: {DbUserId}", dbUser.Id);
                
                return new User(dbUser.Id, dbUser.FirstName, dbUser.LastName, dbUser.Address, dbUserDefaultSearchProfile?.JobTitles, dbUserDefaultSearchProfile?.Regions, dbUser.Created, dbUser.Modified);
            }
            
            async private Task<Models.User> GetAuthenticatedUser(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == request.ClaimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);
                    return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", request.ClaimsUserId, request.ClaimsIssuer);
                    throw new NotAuthorizedException("User could not be identified as a valid user.", e);
                }
            }
            
            
        }
    [SwaggerSchema(Title = "UpdateUserResponse")]
    public record User(Guid Id, string? FirstName, string? LastName, string? address, List<string>? JobTitles, List<string>? Regions, DateTime Created, DateTime Modified);
}