using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public class UpdateUser
{
    [SwaggerSchema(Title = "UpdateUser")]
    public class UpdateUserCommand : IRequest<User>
    {
        public string? FirstName { get; }
        public string? LastName { get; }
        
        public string? Address { get; set; }

        public List<string> JobTitles { get; }
        public List<string> Regions { get; }
        
        [SwaggerIgnore]
        public string? ClaimsUserId { get; set; }
        [SwaggerIgnore]
        public string? ClaimsIssuer { get; set; }

        public UpdateUserCommand(string? firstName, string? lastName, List<string> jobTitles, List<string> regions)
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

    public class Handler : IRequestHandler<UpdateUserCommand, User>
        {
            private readonly UsersDbContext _usersDbContext;
            public Handler(UsersDbContext usersDbContext)
            {
                _usersDbContext = usersDbContext;
            }
            public async Task<User> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                var dbUser = await GetAuthenticatedUser(request, cancellationToken);

                dbUser.FirstName = request.FirstName ?? dbUser.FirstName;
                dbUser.LastName = request.LastName ?? dbUser.LastName;
                dbUser.Address = request.Address ?? dbUser.Address;
                dbUser.Modified = DateTime.UtcNow;
                
                // TODO - To be decided: This default search profile in the user API call can be possibly removed
                var dbUserDefaultSearchProfile = await _usersDbContext.SearchProfiles.FirstOrDefaultAsync(o => o.IsDefault == true && o.UserId == dbUser.Id, cancellationToken);

                if (dbUserDefaultSearchProfile is null)
                {
                    var dbNewSearchProfile = await _usersDbContext.SearchProfiles.AddAsync(new Models.SearchProfile()
                    {
                        Name = request.JobTitles.FirstOrDefault(),
                        UserId = dbUser.Id,
                        JobTitles = request.JobTitles,
                        Regions = request.Regions,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        IsDefault = true
                    }, cancellationToken);
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
                
                return new User(dbUser.Id, dbUser.FirstName, dbUser.LastName, dbUser.Address, dbUserDefaultSearchProfile?.JobTitles, dbUserDefaultSearchProfile?.Regions);
            }
            
            async private Task<Models.User> GetAuthenticatedUser(UpdateUserCommand request, CancellationToken cancellationToken)
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
    [SwaggerSchema("User")]
    public record User(Guid Id, string? FirstName, string? LastName, string? address, List<string>? JobTitles, List<string>? Regions);
}