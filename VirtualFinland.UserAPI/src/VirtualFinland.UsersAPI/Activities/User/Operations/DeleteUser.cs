using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class DeleteUser
{
    public class Command : AuthenticatedRequest
    {

    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly IPersonRepository _personRepository;
        private readonly AnalyticsLogger<Handler> _logger;

        public Handler(IPersonRepository personRepository, AnalyticsLoggerFactory loggerFactory)
        {
            _personRepository = personRepository;
            _logger = loggerFactory.CreateAnalyticsLogger<Handler>();
        }

        public async Task<Unit> Handle(Command request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _personRepository.DeletePerson(request.User.PersonId, cancellationToken);
                await _logger.LogAuditLogEvent(AuditLogEvent.Delete, request.User);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
            }

            return Unit.Value;
        }
    }
}
