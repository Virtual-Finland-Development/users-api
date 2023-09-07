using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.TermsOfServiceAgreement;

public static class GetPersonServiceTermsAgreement
{
    [SwaggerSchema(Title = "GetPersonServiceTermsAgreement")]
    public class Query : IRequest<GetPersonServiceTermsAgreementResponse>
    {
        public Query(Guid personId)
        {
            PersonId = personId;
        }

        [SwaggerIgnore]
        public Guid PersonId { get; }
    }

    public class Handler : IRequestHandler<Query, GetPersonServiceTermsAgreementResponse>
    {
        private readonly ITermsOfServiceRepository _termsOfServiceRepository;

        public Handler(ITermsOfServiceRepository termsOfServiceRepository)
        {
            _termsOfServiceRepository = termsOfServiceRepository;
        }

        public async Task<GetPersonServiceTermsAgreementResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch the newest terms of service
            var termsOfService = await _termsOfServiceRepository.GetNewestTermsOfService();

            // Fetch persons existing agreements
            var existingAgreements = await _termsOfServiceRepository.GetAllTermsOfServiceAgreementsByPersonId(request.PersonId);

            // Check if person has accepted any previous versions of the terms of service
            var personHasAcceptedAnyVersions = existingAgreements.Where(t => t.TermsOfServiceId != termsOfService.Id).Any();

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
