using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
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
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(query => query.UserId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Query, Consents>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(IUserRepository userRepository, ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Consents> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await _userRepository.GetUser(request.UserId, cancellationToken);
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