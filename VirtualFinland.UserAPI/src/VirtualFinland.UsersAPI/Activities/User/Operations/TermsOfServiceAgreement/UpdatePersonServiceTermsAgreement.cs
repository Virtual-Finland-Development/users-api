using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Activities.User.Operations.TermsOfServiceAgreement;

public static class UpdatePersonServiceTermsAgreement
{
    public class Command : AuthenticatedRequest<UpdatePersonServiceTermsAgreementResponse>
    {
        public Command(string version, bool accepted)
        {
            Version = version;
            Accepted = accepted;
        }

        public string Version { get; }
        public bool Accepted { get; }
    }

    public class Handler : IRequestHandler<Command, UpdatePersonServiceTermsAgreementResponse>
    {
        private readonly ITermsOfServiceRepository _termsOfServiceRepository;

        public Handler(ITermsOfServiceRepository termsOfServiceRepository)
        {
            _termsOfServiceRepository = termsOfServiceRepository;
        }

        public async Task<UpdatePersonServiceTermsAgreementResponse> Handle(Command request,
            CancellationToken cancellationToken)
        {
            // Fetch the latest terms of service
            var latestTermsOfService = await _termsOfServiceRepository.GetNewestTermsOfService();

            // Fetch the terms of service with the specified version
            var requestedTermsOfService = request.Version == latestTermsOfService.Version ? latestTermsOfService : await _termsOfServiceRepository.GetTermsOfServiceByVersion(request.Version) ?? throw new BadRequestException("Terms of service not found");

            // Fetch persons existing agreements
            var existingAgreements = await _termsOfServiceRepository.GetAllTermsOfServiceAgreementsByPersonId(request.User.PersonId);

            // Resolve the requested person tos agreement
            var requestedTosAgreement = existingAgreements.SingleOrDefault(t => t.TermsOfServiceId == requestedTermsOfService.Id);

            // Output variables
            var accepted = request.Accepted;
            string? version = null;
            DateTime? acceptedAt = null;
            var hasAcceptedLatest = false;

            // Handle the request
            if (accepted)
            {
                acceptedAt = requestedTosAgreement?.AcceptedAt;
                version = requestedTermsOfService.Version;
                hasAcceptedLatest = request.Version == latestTermsOfService.Version || existingAgreements.Any(t => t.TermsOfServiceId == latestTermsOfService.Id);

                if (requestedTosAgreement is null)
                {
                    await _termsOfServiceRepository.AddNewTermsOfServiceAgreement(requestedTermsOfService, request.User.PersonId);
                    acceptedAt = DateTime.UtcNow;
                }
            }
            else
            {
                if (request.Version != latestTermsOfService.Version)
                {
                    hasAcceptedLatest = existingAgreements.Any(t => t.TermsOfServiceId == latestTermsOfService.Id);
                }

                if (requestedTosAgreement is not null)
                {
                    await _termsOfServiceRepository.RemoveTermsOfServiceAgreement(requestedTosAgreement);
                }
            }

            return new UpdatePersonServiceTermsAgreementResponse
            (
                new CurrentTerms(
                    latestTermsOfService.Url,
                    latestTermsOfService.Description,
                    latestTermsOfService.Version
                ),
                version,
                acceptedAt,
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

    [SwaggerSchema(Title = "UpdatePersonServiceTermsAgreementResponse")]
    public record UpdatePersonServiceTermsAgreementResponse(
        CurrentTerms CurrentTerms,
        string? AcceptedVersion,
        DateTime? AcceptedAt,
        bool HasAcceptedLatest
    );
}
