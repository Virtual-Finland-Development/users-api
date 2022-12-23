// ReSharper disable ClassNeverInstantiated.Global

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Education : IEntity
{
    // ReSharper disable once MemberCanBePrivate.Global
    public enum EducationField
    {
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public enum EducationLevel
    {
    }

    public EducationLevel? EducationLevelEnum { get; set; }
    public EducationField? EducationFieldEnum { get; set; }
    public DateTime? GraduationDate { get; set; }
    public string? EducationOrganization { get; set; }
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
