using System.ComponentModel.DataAnnotations.Schema;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Auditable<T> where T : Auditable<T>
{
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    [Column(TypeName = "jsonb")]
    public AuditableMetadata? Metadata { get; set; }

    public Auditable()
    {
        Created = DateTime.Now;
        Modified = DateTime.Now;
    }

    public T MakeAuditAddition(IRequestAuthenticationCandinate user)
    {
        Created = DateTime.Now;
        MakeAuditUpdate(user);
        return (T)this;
    }

    public T MakeAuditUpdate(IRequestAuthenticationCandinate user)
    {
        Modified = DateTime.Now;
        Metadata = new AuditableMetadata(user);
        return (T)this;
    }
}