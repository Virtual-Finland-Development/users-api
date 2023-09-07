using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.TermsOfServiceAgreement;

public static class GetPersonServiceTermsAgreement
{
    [SwaggerSchema(Title = "GetPersonServiceTermsAgreement")]
    public class Query : IRequest<GetPersonServiceTermsAgreementResponse>
    {
        public Query(Guid? userId)
        {
            UserId = userId;
        }

        [SwaggerIgnore]
        public Guid? UserId { get; }
    }

    public class Handler : IRequestHandler<Query, GetPersonServiceTermsAgreementResponse>
    {
        private readonly UsersDbContext _context;

        public Handler(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<GetPersonServiceTermsAgreementResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch person
            var person = await _context.Persons.SingleAsync(p => p.Id == request.UserId, cancellationToken);
            // Fetch the newest terms of service
            var termsOfService = await _context.TermsOfServices.OrderByDescending(t => t.Version).FirstOrDefaultAsync(cancellationToken) ?? throw new BadRequestException("Terms of service not found");

            // Fetch persons existing agreements
            var existingAgreements = await _context.PersonTermsOfServiceAgreements
                .Where(t => t.PersonId == person.Id)
                .ToListAsync(cancellationToken);

            // Check if person has accepted any previous versions of the terms of service
            var previousAgreements = existingAgreements.Where(t => t.TermsOfService.Version != termsOfService.Version);
            var personHasAcceptedAnyVersions = previousAgreements.Any();

            // Fetch the current person tos agreement
            var currentTosAgreement = existingAgreements.SingleOrDefault(t => t.TermsOfServiceId == termsOfService.Id);

            // Has accepted
            var personHasAccepted = currentTosAgreement != null;

            // Handle the request
            return new GetPersonServiceTermsAgreementResponse(
                termsOfService.Url,
                termsOfService.Description,
                termsOfService.Version,
                personHasAccepted,
                currentTosAgreement?.AcceptedAt ?? null,
                personHasAcceptedAnyVersions
            );
        }
    }

    [SwaggerSchema(Title = "GetPersonServiceTermsAgreementResponse")]
    public record GetPersonServiceTermsAgreementResponse(
        string TermsOfServiceUrl,
        string Description,
        string Version,
        bool Accepted,
        DateTime? AcceptedAt,
        bool AcceptedPreviousVersion
    );
}
