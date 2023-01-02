using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using VirtualFinland.UserAPI.Models.Shared;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

// ReSharper disable once MemberCanBePrivate.Global
public class WorkPreferences : Auditable, IEntity
{
    public List<string>? PreferredRegionEnum { get; set; }
    public List<string>? PreferredMunicipalityEnum { get; set; }
    public string? EmploymentTypeCode { get; set; }
    public WorkingTime? WorkingTimeEnum { get; set; }
    public string? WorkingLanguageEnum { get; set; }

    // Relationships
    [JsonIgnore]
    public User? User { get; set; }

    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ForeignKey(nameof(User))]
    public Guid Id { get; set; }
}
