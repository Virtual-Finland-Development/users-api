using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Certification : Auditable<Certification>, IEntity
{
    [MaxLength(512)]
    public string? Name { get; set; }

    [Url]
    public List<string>? EscoUri { get; set; }

    [MaxLength(256)]
    public string? InstitutionName { get; set; }

    public Guid Id { get; set; }
}
