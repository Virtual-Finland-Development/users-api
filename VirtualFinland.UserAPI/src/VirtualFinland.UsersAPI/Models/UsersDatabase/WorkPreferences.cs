// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

// ReSharper disable once MemberCanBePrivate.Global

// ReSharper disable once MemberCanBePrivate.Global
public enum Region
{
}

// ReSharper disable once MemberCanBePrivate.Global
public enum WorkingTime
{
}

public class WorkPreferences : Auditable, IEntity
{
    public ICollection<Region>? PreferredRegionEnum { get; set; }
    public ICollection<Municipality>? PreferredMunicipalityEnum { get; set; }
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
