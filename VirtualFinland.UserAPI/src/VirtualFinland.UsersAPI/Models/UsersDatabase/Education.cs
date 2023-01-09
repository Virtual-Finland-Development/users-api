// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Education : Auditable, IEntity
{
    // ReSharper disable once MemberCanBePrivate.Global
    public enum EducationLevel
    {
        IscedLevel0 = 0,
        IscedLevel1 = 1,
        IscedLevel2 = 2,
        IscedLevel3 = 3,
        IscedLevel4 = 4,
        IscedLevel5 = 5,
        IscedLevel6 = 6,
        IscedLevel7 = 7,
        IscedLevel8 = 8,
        IscedLevel9 = 9
    }
    
    public EducationLevel? EducationLevelEnum { get; set; }
    
    [MaxLength(4)]
    public string? EducationField { get; set; }
    
    public DateTime? GraduationDate { get; set; }
    
    [MaxLength(256)]
    public string? EducationOrganization { get; set; }
    
    public Guid Id { get; set; }
}
