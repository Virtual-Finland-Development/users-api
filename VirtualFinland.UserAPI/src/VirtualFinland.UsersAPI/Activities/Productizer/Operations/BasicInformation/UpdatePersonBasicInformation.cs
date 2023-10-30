using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;

public static class UpdatePersonBasicInformation
{
    public class Command : AuthenticatedRequest<UpdatePersonBasicInformationResponse>
    {
        public Command(string? givenName, string? lastName, string email, string? phoneNumber, string residency)
        {
            GivenName = givenName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Residency = residency;
        }

        public string? GivenName { get; }
        public string? LastName { get; }
        public string Email { get; }
        public string? PhoneNumber { get; }
        public string? Residency { get; }
    }

    public class Handler : IRequestHandler<Command, UpdatePersonBasicInformationResponse>
    {
        private readonly UsersDbContext _context;
        private readonly AnalyticsService<Handler> _logger;

        public Handler(UsersDbContext context, AnalyticsServiceFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateAnalyticsService<Handler>();
        }

        public async Task<UpdatePersonBasicInformationResponse> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var person = await _context.Persons.SingleAsync(p => p.Id == request.User.PersonId, cancellationToken);

            person.GivenName = request.GivenName ?? person.GivenName;
            person.LastName = request.LastName ?? person.LastName;
            person.Email = request.Email;
            person.PhoneNumber = request.PhoneNumber ?? person.PhoneNumber;
            person.ResidencyCode = request.Residency ?? person.ResidencyCode;

            try
            {
                await _context.SaveChangesAsync(request.User, cancellationToken);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
            }

            await _logger.HandleAuditLogEvent(AuditLogEvent.Update, request.User);

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
