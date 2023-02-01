using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;

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

        public string? GivenName { get; }
        public string? LastName { get; }
        public string Email { get; }
        public string? PhoneNumber { get; }
        public string? Residency { get; }

        public void SetAuth(Guid? userDatabaseId)
        {
            UserId = userDatabaseId;
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
            var person = await _context.Persons.SingleAsync(p => p.Id == request.UserId, cancellationToken);

            person.GivenName = request.GivenName ?? person.GivenName;
            person.LastName = request.LastName ?? person.LastName;
            person.Email = request.Email;
            person.PhoneNumber = request.PhoneNumber ?? person.PhoneNumber;
            person.ResidencyCode = request.Residency ?? person.ResidencyCode;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdatePersonBasicInformationResponse
            (
                person.GivenName,
                person.LastName,
                person.Email,
                person.PhoneNumber,
                person.ResidencyCode
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
