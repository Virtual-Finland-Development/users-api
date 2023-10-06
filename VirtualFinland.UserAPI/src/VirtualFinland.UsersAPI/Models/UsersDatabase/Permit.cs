using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Permit : Auditable<Permit>, IEntity
{
    /// <summary>
    ///     http://uri.suomi.fi/codelist/dataecon/permit
    /// </summary>
    [MaxLength(3)]
    public string? TypeCode { get; set; }

    public Guid Id { get; set; }
}
