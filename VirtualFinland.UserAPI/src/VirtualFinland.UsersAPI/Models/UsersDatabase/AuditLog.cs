using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class AuditLog
{
    public string TableName { get; init; } = default!;
    public string Action { get; init; } = default!;
    public string KeyValues { get; init; } = default!;
    public string ChangedColumns { get; init; } = default!;
    public DateTime EventDate { get; init; } = default!;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

}