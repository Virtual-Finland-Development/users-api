using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class TermsOfService : Auditable, IEntity
{
    public Guid Id { get; set; }
    public string Version { get; set; } = default!;
    [Url]
    public string Url { get; set; } = default!;
    public string Description { get; set; } = default!;
}
