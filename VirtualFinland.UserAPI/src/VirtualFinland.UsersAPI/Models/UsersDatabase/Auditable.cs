using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public abstract class Auditable
{
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    [Column(TypeName = "jsonb")]
    public AuditableMetadata? Metadata { get; set; }
}