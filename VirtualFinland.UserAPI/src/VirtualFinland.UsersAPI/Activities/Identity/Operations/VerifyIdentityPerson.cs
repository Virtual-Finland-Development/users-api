using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.Identity.Operations;

public static class VerifyIdentityPerson
{
    [SwaggerSchema(Title = "TestbedIdentityUserRequest")]
    public class Query : IRequest<User>
    {
        public Query(HttpContext context)
        {
            Context = context;
        }

        public HttpContext Context { get; }
    }

    public class Handler : IRequestHandler<Query, User>
    {
        private readonly AuthenticationService _authenticationService;
        private readonly AnalyticsLogger<Handler> _logger;

        public Handler(AuthenticationService authenticationService, AnalyticsLoggerFactory loggerFactory)
        {
            _authenticationService = authenticationService;
            _logger = loggerFactory.CreateAnalyticsLogger<Handler>();
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var person = await _authenticationService.AuthenticateAndGetOrRegisterAndGetPerson(request.Context, cancellationToken);
            await _logger.LogAuditLogEvent(AuditLogEvent.Read, request.Context.Items["User"] as RequestAuthenticatedUser ?? throw new Exception("Unknown error occurred on verifying identity"));
            return new User(person.Id, person.Created, person.Modified);
        }
    }

    [SwaggerSchema(Title = "TestbedIdentityUserResponse")]
    public record User(Guid Id, DateTime Created, DateTime Modified);
}
