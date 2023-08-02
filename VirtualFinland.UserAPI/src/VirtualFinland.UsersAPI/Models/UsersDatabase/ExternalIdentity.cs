using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class ExternalIdentity : Encryptable, IEntity
{
    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string IdentityHash { get; set; } = null!;

    [Required]
    [Encrypted]
    public string KeyToPersonDataAccessKey { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
