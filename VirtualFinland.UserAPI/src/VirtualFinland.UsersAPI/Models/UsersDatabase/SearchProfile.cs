using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class SearchProfile : Auditable, IEntity
{
    [Required]
    public Guid PersonId { get; set; }

    public string? Name { get; set; }

    public List<string>? JobTitles { get; set; }
    public List<string>? Regions { get; set; }

    public bool IsDefault { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
