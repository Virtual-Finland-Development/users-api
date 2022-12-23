// ReSharper disable ClassNeverInstantiated.Global

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Language
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

    public string? EscoUri { get; set; }
    public string? LanguageCode { get; set; }
    public SkillLevel? SkillLevelEnum { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
