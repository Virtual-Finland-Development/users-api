using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
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
        public Guid? UserId { get; set; }

        public string Version { get; }
        public bool Accepted { get; }

        public void SetAuth(Guid? userDatabaseId)
        {
            UserId = userDatabaseId;
        }
    }

    public class Handler : IRequestHandler<Command, UpdatePersonServiceTermsAgreementResponse>
    {
        private readonly UsersDbContext _context;

        public Handler(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<UpdatePersonServiceTermsAgreementResponse> Handle(Command request,
            CancellationToken cancellationToken)
        {
            // Fetch person
            var person = await _context.Persons.SingleAsync(p => p.Id == request.UserId, cancellationToken);
            // Fetch the terms of service with the specified version
            var termsOfService = await _context.TermsOfServices.SingleOrDefaultAsync(t => t.Version == request.Version, cancellationToken) ?? throw new BadRequestException("Terms of service not found");

            // Fetch persons existing agreements
            var existingAgreements = await _context.PersonTermsOfServiceAgreements
                .Where(t => t.PersonId == person.Id)
                .ToListAsync(cancellationToken);

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
                    person.TermsOfServiceAgreements.Add(new PersonTermsOfServiceAgreement
                    {
                        TermsOfServiceId = termsOfService.Id,
                    });
                    acceptedAt = DateTime.UtcNow;
                }
            }
            else
            {
                if (currentTosAgreement is not null)
                {
                    _context.PersonTermsOfServiceAgreements.Remove(currentTosAgreement);
                }
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
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
