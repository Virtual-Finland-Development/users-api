using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class WorkPreferences : Auditable<WorkPreferences>, IEntity
{
    /// <summary>
    ///     Region.cs values
    /// </summary>
    public ICollection<string>? PreferredRegionCode { get; set; }

    /// <summary>
    ///     Municipality.cs values
    /// </summary>
    public ICollection<string>? PreferredMunicipalityCode { get; set; }

    public string? EmploymentTypeCode { get; set; }

    /// <summary>
    ///     WorkingTime.cs values
    /// </summary>
    public string? WorkingTimeCode { get; set; }

    /// <summary>
    ///     WorkingLanguage.cs values
    /// </summary>
    public ICollection<string>? WorkingLanguageEnum { get; set; }

    /// <summary>
    ///     http://uri.suomi.fi/codelist/jhs/toimiala_1_20080101
    /// </summary>
    [MaxLength(7)]
    public string? NaceCode { get; set; }

    [JsonIgnore]
    public Person Person { get; set; } = null!;

    [Key]
    [Required]
    [ForeignKey(nameof(Person))]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
