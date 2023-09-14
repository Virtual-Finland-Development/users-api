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
        public Query(Guid personId, string audience)
        {
            PersonId = personId;
            Audience = audience;
        }

        [SwaggerIgnore]
        public Guid PersonId { get; }
        [SwaggerIgnore]
        public string Audience { get; set; } = default!;
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

            // Fetch the persons latest agreement
            var latestExistingAgreement = await _termsOfServiceRepository.GetTheLatestTermsOfServiceAgreementByPersonId(request.PersonId, request.Audience);

            // Has accepted
            var hasAcceptedLatest = latestExistingAgreement?.TermsOfServiceId == termsOfService.Id;

            // Handle the request
            return new GetPersonServiceTermsAgreementResponse(
                new CurrentTerms(
                    termsOfService.Url,
                    termsOfService.Description,
                    termsOfService.Version
                ),
                latestExistingAgreement?.Version,
                latestExistingAgreement?.AcceptedAt,
                hasAcceptedLatest
            );
        }
    }

    [SwaggerSchema(Title = "CurrentTerms")]
    public record CurrentTerms(
        string Url,
        string Description,
        string Version
    );

    [SwaggerSchema(Title = "GetPersonServiceTermsAgreementResponse")]
    public record GetPersonServiceTermsAgreementResponse(
        CurrentTerms CurrentTerms,
        string? AcceptedVersion,
        DateTime? AcceptedAt,
        bool HasAcceptedLatest
    );
}
