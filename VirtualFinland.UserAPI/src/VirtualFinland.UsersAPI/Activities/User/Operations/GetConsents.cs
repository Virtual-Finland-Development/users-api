using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class GetConsents
{
    [SwaggerSchema(Title = "ConsentsRequest")]
    public class Query : IRequest<Consents>
    {
        [SwaggerIgnore]
        public Guid? UserId { get; }

        public Query(Guid? userId)
        {
            this.UserId = userId;
        }
    }

    public class Handler : IRequestHandler<Query, Consents>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<Consents> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await _usersDbContext.Users.SingleAsync(o => o.Id == request.UserId, cancellationToken: cancellationToken);
            _logger.LogDebug("User consents retrieved for user: {DbUserId}", dbUser.Id);
            
            return new Consents(
                dbUser.ImmigrationDataConsent,
                dbUser.JobsDataConsent
                );
        }
    }
    
    [SwaggerSchema(Title = "ConsentsResponse")]
    public record Consents(
        bool ImmigrationDataConsent,
        bool JobsDataConsent
        );
    
}