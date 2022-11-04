using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Activities.Identity.Operations;

public static class VerifyIdentityUser
{
    [SwaggerSchema(Title = "TestbedIdentityUserRequest")]
    public class Query : IRequest<User>
    {
        public string? ClaimsUserId { get; }
        public string? ClaimsIssuer { get; }
        
        public Query(string? claimsUserId, string? claimsIssuer)
        {
            this.ClaimsUserId = claimsUserId;
            this.ClaimsIssuer = claimsIssuer;
        }
    }

    public class Handler : IRequestHandler<Query, User>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;
        private readonly IConfiguration _configuration;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger, IConfiguration configuration)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var claimsUserId = request.ClaimsUserId;
            
            if (request.ClaimsIssuer != null && request.ClaimsIssuer.Contains(_configuration["SuomiFI:Issuer"]))
            {
                claimsUserId = "suomifiDummyUserId";
            }
            
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleOrDefaultAsync(o => o.IdentityId == claimsUserId && o.Issuer == request.ClaimsIssuer, cancellationToken);

            // Create a new system user is no one found based on given authentication information
            if (externalIdentity is null)
            {
                var newDbUSer = await _usersDbContext.Users.AddAsync(new Models.UsersDatabase.User()
                { Created = DateTime.UtcNow, Modified = DateTime.UtcNow }, cancellationToken);

                await _usersDbContext.ExternalIdentities.AddAsync(new ExternalIdentity()
                {
                    Issuer = request.ClaimsIssuer,
                    IdentityId = claimsUserId,
                    UserId = newDbUSer.Entity.Id,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                }, cancellationToken);
                

                await _usersDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Verified and created a new user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", claimsUserId, request.ClaimsIssuer);
                return new User(newDbUSer.Entity.Id, newDbUSer.Entity.Created, newDbUSer.Entity.Modified);
            }
            
            var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, cancellationToken);
            
            _logger.LogInformation("Verified an existing user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", claimsUserId, request.ClaimsIssuer);
            return new User(dbUser.Id, dbUser.Created, dbUser.Modified);
        }
    }

    [SwaggerSchema(Title ="TestbedIdentityUserResponse")]
    public record User(Guid Id, DateTime Created, DateTime Modified);
}