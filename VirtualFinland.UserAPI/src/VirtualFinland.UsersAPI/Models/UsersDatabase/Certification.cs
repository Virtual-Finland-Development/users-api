// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Certification : Auditable, IEntity
{
    [MaxLength(512)]
    public string? Name { get; set; }
    
    [MaxLength(256)]
    public string? Type { get; set; }
    
    public Guid Id { get; set; }
}
