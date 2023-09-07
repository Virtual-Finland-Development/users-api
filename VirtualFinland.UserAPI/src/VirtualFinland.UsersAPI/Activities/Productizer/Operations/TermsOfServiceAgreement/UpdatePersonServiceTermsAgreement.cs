using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.TermsOfServiceAgreement;

public static class UpdatePersonServiceTermsAgreement
{
    public class Command : IRequest<UpdatePersonServiceTermsAgreementResponse>
    {
        public Command(string version, bool accepted)
        {
            Version = version;
            Accepted = accepted;
        }

        [SwaggerIgnore]
        public Guid PersonId { get; set; }

        public string Version { get; }
        public bool Accepted { get; }

        public void SetAuth(Guid personId)
        {
            PersonId = personId;
        }
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
            // Fetch the terms of service with the specified version
            var termsOfService = await _termsOfServiceRepository.GetTermsOfServiceByVersion(request.Version) ?? throw new BadRequestException("Terms of service not found");

            // Fetch persons existing agreements
            var existingAgreements = await _termsOfServiceRepository.GetAllTermsOfServiceAgreementsByPersonId(request.PersonId);

            // Check if person has accepted any previous versions of the terms of service
            var personHasAcceptedAnyVersions = existingAgreements.Where(t => t.TermsOfServiceId != termsOfService.Id).Any();

            // Fetch the current person tos agreement
            var currentTosAgreement = existingAgreements.SingleOrDefault(t => t.TermsOfServiceId == termsOfService.Id);

            // Output variables
            var accepted = request.Accepted;
            DateTime? acceptedAt = null;

            // Handle the request
            if (accepted)
            {
                acceptedAt = currentTosAgreement?.AcceptedAt;

                if (currentTosAgreement is null)
                {
                    await _termsOfServiceRepository.AddNewTermsOfServiceAgreement(termsOfService, request.PersonId);
                    acceptedAt = DateTime.UtcNow;
                }
            }
            else
            {
                if (currentTosAgreement is not null)
                {
                    await _termsOfServiceRepository.RemoveTermsOfServiceAgreement(currentTosAgreement);
                }
            }

            return new UpdatePersonServiceTermsAgreementResponse
            (
                termsOfService.Url,
                termsOfService.Description,
                termsOfService.Version,
                accepted,
                acceptedAt,
                personHasAcceptedAnyVersions
            );
        }
    }

    [SwaggerSchema(Title = "UpdatePersonServiceTermsAgreementResponse")]
    public record UpdatePersonServiceTermsAgreementResponse(
        string TermsOfServiceUrl,
        string Description,
        string Version,
        bool Accepted,
        DateTime? AcceptedAt,
        bool AcceptedPreviousVersion
    );
}
