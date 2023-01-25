using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class GetSearchProfiles
{
    [SwaggerSchema(Title = "SearchProfilesRequest")]
    public class Query : IRequest<IList<SearchProfile>>
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

    public class Handler : IRequestHandler<Query, IList<SearchProfile>>
    {
        private readonly UsersDbContext _usersDbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext usersDbContext, ILogger<Handler> logger)
        {
            _usersDbContext = usersDbContext;
            _logger = logger;
        }

        public async Task<IList<SearchProfile>> Handle(Query request, CancellationToken cancellationToken)
        {
            var dbUser = await _usersDbContext.Persons.SingleAsync(o => o.Id == request.UserId, cancellationToken: cancellationToken);

            var userSearchProfiles = _usersDbContext.SearchProfiles.Where(o => o.PersonId == dbUser.Id);
            
            _logger.LogDebug("Retrieving search profiles");

            return await userSearchProfiles.Select(o => new SearchProfile(o.Id, o.JobTitles, o.Name, o.Regions, o.Created, o.Modified)).ToListAsync(cancellationToken);
        }
    }
    
    [SwaggerSchema(Title = "SearchProfilesResponse")]

    public record SearchProfile(Guid Id, List<string>? JobTitles, string? Name, List<string>? Regions, DateTime Created, DateTime Modified);
}

