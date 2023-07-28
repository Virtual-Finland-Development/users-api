using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;

public static class UpdatePersonBasicInformation
{
    public class Command : IRequest<UpdatePersonBasicInformationResponse>
    {
        public Command(string? givenName, string? lastName, string email, string? phoneNumber, string residency)
        {
            GivenName = givenName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Residency = residency;
        }

        [SwaggerIgnore]
        public Guid? UserId { get; set; }
        public string? EncryptionKey { get; set; }

        public string? GivenName { get; }
        public string? LastName { get; }
        public string Email { get; }
        public string? PhoneNumber { get; }
        public string? Residency { get; }

        public void SetAuth(Guid? userDatabaseId, string? encryptionKey)
        {
            UserId = userDatabaseId;
            EncryptionKey = encryptionKey;
        }
    }

    public class Handler : IRequestHandler<Command, UpdatePersonBasicInformationResponse>
    {
        private readonly UsersDbContext _context;

        public Handler(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<UpdatePersonBasicInformationResponse> Handle(Command request,
            CancellationToken cancellationToken)
        {
            _context.Cryptor.State.StartQuery("Person", request.EncryptionKey);
            var person = await _context.Persons.SingleAsync(p => p.Id == request.UserId, cancellationToken);

            person.GivenName = request.GivenName ?? person.GivenName;
            person.LastName = request.LastName ?? person.LastName;
            person.Email = request.Email;
            person.PhoneNumber = request.PhoneNumber ?? person.PhoneNumber;
            person.ResidencyCode = request.Residency ?? person.ResidencyCode;

            // Deep clone the user object to avoid EF tracking/encryption issues
            var updatedPerson = person.Clone() as Person;
            if (updatedPerson == null)
            {
                throw new ArgumentException("Failed to clone user object");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
            }

            return new UpdatePersonBasicInformationResponse
            (
                updatedPerson.GivenName,
                updatedPerson.LastName,
                updatedPerson.Email ?? request.Email, // @TODO fix requirement
                updatedPerson.PhoneNumber,
                updatedPerson.ResidencyCode
            );
        }
    }

    [SwaggerSchema(Title = "UpdatePersonBasicInformationResponse")]
    public record UpdatePersonBasicInformationResponse(
        string? GivenName,
        string? LastName,
        string Email,
        string? PhoneNumber,
        string? Residency
    );
}
