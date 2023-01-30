using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class WorkPreferences : Auditable, IEntity
{
    /// <summary>
    ///     Region.cs values
    /// </summary>
    public ICollection<string> PreferredRegionCode { get; set; } = null!;

    /// <summary>
    ///     Municipality.cs values
    /// </summary>
    public ICollection<string> PreferredMunicipalityCode { get; set; } = null!;

    public string? EmploymentTypeCode { get; set; }

    /// <summary>
    ///     WorkingTime.cs values
    /// </summary>
    public string? WorkingTimeCode { get; set; }

    /// <summary>
    ///     Possible values are "fi", "en", "sv"
    /// </summary>
    [MaxLength(2)]
    public string? WorkingLanguageEnum { get; set; }

    [JsonIgnore]
    public Person Person { get; set; } = null!;

    [Key]
    [Required]
    [ForeignKey(nameof(Person))]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
