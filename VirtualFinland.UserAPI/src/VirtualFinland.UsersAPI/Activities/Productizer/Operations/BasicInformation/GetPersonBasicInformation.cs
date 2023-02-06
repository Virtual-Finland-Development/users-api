using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;

public static class GetPersonBasicInformation
{
    [SwaggerSchema(Title = "GetPersonBasicInformationRequest")]
    public class Query : IRequest<GetPersonBasicInformationResponse>
    {
        public Query(Guid? userId)
        {
            UserId = userId;
        }

        [SwaggerIgnore]
        public Guid? UserId { get; }
    }

    public class Handler : IRequestHandler<Query, GetPersonBasicInformationResponse>
    {
        private readonly UsersDbContext _context;

        public Handler(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<GetPersonBasicInformationResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var person = await _context.Persons.SingleAsync(p => p.Id == request.UserId, cancellationToken);

            return new GetPersonBasicInformationResponse(
                person.GivenName,
                person.LastName,
                person.Email,
                person.PhoneNumber,
                person.ResidencyCode
            );
        }
    }

    [SwaggerSchema(Title = "GetPersonBasicInformationResponse")]
    public record GetPersonBasicInformationResponse(
        string? GivenName,
        string? LastName,
        string? Email,
        string? PhoneNumber,
        string? Residency
    );
}
