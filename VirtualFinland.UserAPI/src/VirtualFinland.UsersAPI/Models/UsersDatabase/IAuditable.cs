using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public interface IAuditable
{
    DateTime Created { get; set; }
    DateTime Modified { get; set; }
    AuditableMetadata? Metadata { get; set; }
    void SetupAuditEvent(UsersDbContext dbContext, IRequestAuthenticationCandinate user);
}