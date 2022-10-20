using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models;

public class ExternalIdentity : IEntity
{

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    [Required]
    public string? Issuer { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string? IdentityId { get; set; }
}