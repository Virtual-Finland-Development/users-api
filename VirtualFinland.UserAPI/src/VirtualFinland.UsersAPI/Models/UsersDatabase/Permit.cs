// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Permit : Auditable, IEntity
{
    [MaxLength(255)]
    public string? Name { get; set; }

    // TODO: Should this be enum?
    [MaxLength(255)]
    public string? Type { get; set; }

    public Guid Id { get; set; }
}
