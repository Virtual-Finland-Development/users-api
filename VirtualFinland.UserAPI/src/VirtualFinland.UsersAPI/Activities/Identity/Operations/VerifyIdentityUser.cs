using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.UserAPI.Activities.Identity.Operations;

public static class VerifyIdentityUser
{
    [SwaggerSchema(Title = "TestbedIdentityUserRequest")]
    public class Query : IRequest<User>
    {
        public Query(string? claimsUserId, string? claimsIssuer)
        {
            ClaimsUserId = claimsUserId;
            ClaimsIssuer = claimsIssuer;
        }

        public string? ClaimsUserId { get; }
        public string? ClaimsIssuer { get; }
    }

    public class Handler : IRequestHandler<Query, User>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IPersonsRepository _personsRepository;

        public Handler(IPersonsRepository personsRepository, ILogger<Handler> logger)
        {
            _personsRepository = personsRepository;
            _logger = logger;
        }

        public async Task<User> Handle(Query request, CancellationToken cancellationToken)
        {
            var claimsIssuer = request.ClaimsIssuer ?? throw new ArgumentNullException(nameof(request.ClaimsIssuer));
            var claimsUserId = request.ClaimsUserId ?? throw new ArgumentNullException(nameof(request.ClaimsUserId));
            var person = await _personsRepository.GetOrCreatePerson(claimsIssuer, claimsUserId, cancellationToken);
            return new User(person.Id, person.Created, person.Modified);
        }
    }

    [SwaggerSchema(Title = "TestbedIdentityUserResponse")]
    public record User(Guid Id, DateTime Created, DateTime Modified);
}
