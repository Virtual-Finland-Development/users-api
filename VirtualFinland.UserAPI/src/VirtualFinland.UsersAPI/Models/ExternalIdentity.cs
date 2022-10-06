using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models;

public class ExternalIdentity : IEntity
{

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public string Issuer { get; set; }
    
    public Guid UserId { get; set; }
    
    public string IdentityId { get; set; }
}