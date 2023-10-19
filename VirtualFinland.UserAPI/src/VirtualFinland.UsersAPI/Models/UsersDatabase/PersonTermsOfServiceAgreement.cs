using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class PersonTermsOfServiceAgreement : IEntity
{
    public Person Person { get; set; } = null!;
    public TermsOfService TermsOfService { get; set; } = null!;
    public string Version { get; set; } = default!;
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;

    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Person))]
    public Guid PersonId { get; set; }
    [ForeignKey(nameof(TermsOfService))]
    public Guid TermsOfServiceId { get; set; }
}
