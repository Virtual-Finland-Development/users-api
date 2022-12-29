// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Language : IEntity
{
    // ReSharper disable once MemberCanBePrivate.Global
    public enum SkillLevel
    {
        A1,
        A2,
        B1,
        B2,
        C1,
        C2
    }

    [Url]
    public string? EscoUri { get; set; }
    
    [MaxLength(3)]
    public string? LanguageCode { get; set; }
    
    public SkillLevel? SkillLevelEnum { get; set; }
    
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
