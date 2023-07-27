using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class ExternalIdentity : IEntity, IEncrypted
{
    [Required]
    public string? Issuer { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    //[Encrypted]
    public string? IdentityId { get; set; }

    [Required]
    public string? IdentityHash { get; set; }

    //[Required]
    //[Encrypted]
    //public ExternalIdentityAccessKey? IdentityAccessKey { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
