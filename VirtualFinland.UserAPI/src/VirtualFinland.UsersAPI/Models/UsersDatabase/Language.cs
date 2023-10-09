using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Language : Auditable, IEntity
{
    /// <summary>
    ///     http://uri.suomi.fi/codelist/dataecon/cerf
    /// </summary>
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

    public string? CerfCode { get; set; }

    public Guid Id { get; set; }
}
