using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.User.Operations;

public static class UpdateConsents
{
    [SwaggerSchema(Title = "UpdateConsentsRequest")]
    public class Command : IRequest<Consents>
    {
        public bool? JobsDataConsent { get; set; }
        
        public bool? ImmigrationDataConsent { get; set; }

        [SwaggerIgnore]
        public Guid? UserId { get; private set; }


        public Command(bool? jobsDataConsent, bool? immigrationDataConsent)
        {
            this.JobsDataConsent = jobsDataConsent;
            this.ImmigrationDataConsent = immigrationDataConsent;
        }

        public void SetAuth(Guid? userDbId)
        {
            this.UserId = userDbId;
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.UserId).NotNull().NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, Consents>
        {
            private readonly IUserRepository _userRepository;
            private readonly ILogger<Handler> _logger;

            public Handler(IUserRepository userRepository, ILogger<Handler> logger)
            {
                _userRepository = userRepository;
                _logger = logger;
            }

            public async Task<Consents> Handle(Command request, CancellationToken cancellationToken)
            {
                var dbUser = await _userRepository.GetUser(request.UserId, cancellationToken);
                dbUser.Modified = DateTime.UtcNow;
                dbUser.ImmigrationDataConsent = request.ImmigrationDataConsent ?? dbUser.ImmigrationDataConsent;
                dbUser.JobsDataConsent = request.JobsDataConsent ?? dbUser.JobsDataConsent;

                await _userRepository.UpdateUser(dbUser, cancellationToken);
                
                _logger.LogDebug("User data updated for user: {DbUserId}", dbUser.Id);

                return new Consents(
                    dbUser.ImmigrationDataConsent,
                    dbUser.JobsDataConsent);
            }
        }
    [SwaggerSchema(Title = "UpdateConsentsResponse")]
    public record Consents(
        bool ImmigrationDataConsent,
        bool JobsDataConsent);
}