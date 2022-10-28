using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace VirtualFinland.UserAPI.Activities.CodeSets.Operations;

public static class GetAllGenders
{
    [SwaggerSchema(Title = "GenderCodeSetRequest")]
    public class Query : IRequest<List<Gender>>
    {
        
    }

    public class Handler : IRequestHandler<Query, List<Gender>>
    {

        public Task<List<Gender>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Gender>()
            {
                new Gender("1", "Female"),
                new Gender("2", "Male"),
                new Gender("3", "Other"),
                new Gender("4", "Prefer not to say")
            });
        }
    }

    [SwaggerSchema(Title = "GenderCodeSetResponse")]
    public record Gender(string Id, string DisplayName);
}

