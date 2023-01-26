using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class WorkPreferences : Auditable, IEntity
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
    ///     Possible values are "fi", "en", "sv"
    /// </summary>
    [MaxLength(2)]
    public string? WorkingLanguageEnum { get; set; }

    [Required]
    public Guid PersonId { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
