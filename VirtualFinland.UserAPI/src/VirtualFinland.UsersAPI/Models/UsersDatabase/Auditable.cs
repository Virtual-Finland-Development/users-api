using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Auditable : IAuditable
{
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    [Column(TypeName = "jsonb")]
    public AuditableMetadata? Metadata { get; set; }

    public void SetupAuditEvent(UsersDbContext dbContext, IRequestAuthenticationCandinate user)
    {
        var entry = dbContext.Entry(this);
        switch (entry?.State)
        {
            case EntityState.Added:
                SetupAuditAddition(user);
                break;
            case EntityState.Deleted:
            case EntityState.Modified:
                SetupAuditUpdate(user);
                break;
        }
    }

    protected void SetupAuditAddition(IRequestAuthenticationCandinate user)
    {
        Created = DateTime.Now;
        SetupAuditUpdate(user);
    }

    protected void SetupAuditUpdate(IRequestAuthenticationCandinate user)
    {
        Modified = DateTime.Now;
        Metadata = new AuditableMetadata(user);
    }
}