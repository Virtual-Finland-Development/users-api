using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Helpers.Extensions;
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
        private readonly ILogger<Handler> _logger;

        public Handler(AuthenticationService authenticationService, ILogger<Handler> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var person = await _authenticationService.AuthenticateAndGetOrRegisterAndGetPerson(request.Context, cancellationToken);
            _logger.LogAuditLogEvent(AuditLogEvent.Read, "Identity", request.Context.Items["User"] as RequestAuthenticatedUser ?? throw new Exception("Unknown error occurred on verifying identity"));
            return new User(person.Id, person.Created, person.Modified);
        }
    }

    [SwaggerSchema(Title = "TestbedIdentityUserResponse")]
    public record User(Guid Id, DateTime Created, DateTime Modified);
}
