using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Education : Auditable, IEntity
{
    /// <summary>
    ///     http://uri.suomi.fi/codelist/dataecon/educationlevel
    /// </summary>
    public enum EducationLevel
    {
        [EnumMember(Value = "0")] IscedLevel0 = 0,
        [EnumMember(Value = "1")] IscedLevel1 = 1,
        [EnumMember(Value = "2")] IscedLevel2 = 2,
        [EnumMember(Value = "3")] IscedLevel3 = 3,
        [EnumMember(Value = "4")] IscedLevel4 = 4,
        [EnumMember(Value = "5")] IscedLevel5 = 5,
        [EnumMember(Value = "6")] IscedLevel6 = 6,
        [EnumMember(Value = "7")] IscedLevel7 = 7,
        [EnumMember(Value = "8")] IscedLevel8 = 8,
        [EnumMember(Value = "9")] IscedLevel9 = 9
    }

    [MaxLength(256)]
    public string? Name { get; set; }
    
    [MaxLength(1)]
    public string? EducationLevelCode { get; set; }

    /// <summary>
    ///     http://uri.suomi.fi/codelist/jhs/isced_ala_1_20110101
    /// </summary>
    [MinLength(2)]
    [MaxLength(4)]
    public string? EducationFieldCode { get; set; }

    public DateTime? GraduationDate { get; set; }

    [MaxLength(256)]
    public string? InstitutionName { get; set; }
    
    public ICollection<Skills>? Skills { get; set; }

    public Guid Id { get; set; }
}
