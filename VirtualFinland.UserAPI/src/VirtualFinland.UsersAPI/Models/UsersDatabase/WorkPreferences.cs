using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtualFinland.UserAPI.Models.Shared;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class WorkPreferences : Auditable, IEntity
{
    /// <summary>
    ///     Region.cs values
    /// </summary>
    public ICollection<Region>? PreferredRegionCode { get; set; }

    /// <summary>
    ///     Municipality.cs values
    /// </summary>
    public ICollection<Municipality>? PreferredMunicipalityCode { get; set; }

    public EmploymentType? EmploymentTypeCode { get; set; }

    /// <summary>
    ///     WorkingTime.cs values
    /// </summary>
    public WorkingTime? WorkingTimeCode { get; set; }

    /// <summary>
    ///     Possible values are "fi", "en", "sv"
    /// </summary>
    [MaxLength(2)]
    public WorkingLanguage? WorkingLanguageEnum { get; set; }

    [Required]
    public Guid PersonId { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
