using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class TermsOfService : Auditable, IEntity
{
    [Url]
    public string Url { get; set; } = default!;
    public string Description { get; set; } = default!;
    public List<Person> Persons { get; } = new();

    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public string Version { get; set; } = default!;
}
