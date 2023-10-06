using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Auditable<T> where T : Auditable<T>
{
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    [Column(TypeName = "jsonb")]
    public AuditableMetadata? Metadata { get; set; }

    public T SetupAuditEvent(EntityState entityState, IRequestAuthenticationCandinate user)
    {
        switch (entityState)
        {
            case EntityState.Added:
                SetupAuditAddition(user);
                break;
            case EntityState.Deleted:
            case EntityState.Modified:
                SetupAuditUpdate(user);
                break;
        }
        return (T)this;
    }

    public T SetupAuditAddition(IRequestAuthenticationCandinate user)
    {
        Created = DateTime.Now;
        SetupAuditUpdate(user);
        return (T)this;
    }

    public T SetupAuditUpdate(IRequestAuthenticationCandinate user)
    {
        Modified = DateTime.Now;
        Metadata = new AuditableMetadata(user);
        return (T)this;
    }
}