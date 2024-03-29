using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;

public static class GetPersonBasicInformation
{
    [SwaggerSchema(Title = "GetPersonBasicInformationRequest")]
    public class Query : AuthenticatedRequest<GetPersonBasicInformationResponse>
    {
        public Query(RequestAuthenticatedUser RequestAuthenticatedUser) : base(RequestAuthenticatedUser)
        {
        }
    }

    public class Handler : IRequestHandler<Query, GetPersonBasicInformationResponse>
    {
        private readonly UsersDbContext _context;
        private readonly AnalyticsLogger<Handler> _logger;

        public Handler(UsersDbContext context, AnalyticsLoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateAnalyticsLogger<Handler>();
        }

        public async Task<GetPersonBasicInformationResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var person = await _context.Persons.SingleAsync(p => p.Id == request.User.PersonId, cancellationToken);

            await _logger.LogAuditLogEvent(AuditLogEvent.Read, request.User);

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
