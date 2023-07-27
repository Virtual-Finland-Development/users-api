using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class ExternalIdentityAccessKey : Auditable, IEntity
{
    [Required]
    //[Encrypted]
    public string? IdentityAccessKey { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public new DateTime Created { get; set; }
    public new DateTime Modified { get; set; }
}
