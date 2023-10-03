using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Helpers.Services;

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

        public Handler(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var person = await _authenticationService.AuthenticateAndGetOrRegisterAndGetPerson(request.Context);
            return new User(person.Id, person.Created, person.Modified);
        }
    }

    [SwaggerSchema(Title = "TestbedIdentityUserResponse")]
    public record User(Guid Id, DateTime Created, DateTime Modified);
}
