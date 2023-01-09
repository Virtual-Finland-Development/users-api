// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Skills : Auditable, IEntity
{
    // ReSharper disable once MemberCanBePrivate.Global
    public enum SkillLevel
    {
        Beginner,
        Intermediate,
        Master
    }

    [Url]
    public string? EscoUrl { get; set; }

    [MaxLength(3)]
    public string? LanguageCode { get; set; }

    public SkillLevel? SkillLevelEnum { get; set; }

    public Guid Id { get; set; }
}
