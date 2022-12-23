// ReSharper disable ClassNeverInstantiated.Global

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Skills
{
    // ReSharper disable once MemberCanBePrivate.Global
    public enum SkillLevel
    {
        Beginner,
        Intermediate,
        Master
    }

    public string? EscoUrl { get; set; }
    public string? LanguageCode { get; set; }
    public SkillLevel? SkillLevelEnum { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Modified { get; set; }
}
