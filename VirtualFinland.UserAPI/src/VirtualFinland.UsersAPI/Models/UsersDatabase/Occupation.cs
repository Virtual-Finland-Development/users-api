using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Occupation : Auditable, IEntity
{
    [Url]
    public string? EscoUri { get; set; }

    /// <summary>
    ///     http://uri.suomi.fi/codelist/tulorekisteri/OccupationalTitles
    /// </summary>
    [MaxLength(16)]
    public string? EscoCode { get; set; }

    [MaxLength(256)]
    public string? Employer { get; set; }

    [Range(0, 600)]
    public int? WorkMonths { get; set; }

    // Relationships
    public Guid PersonId { get; set; }

    public Guid Id { get; set; }
}
