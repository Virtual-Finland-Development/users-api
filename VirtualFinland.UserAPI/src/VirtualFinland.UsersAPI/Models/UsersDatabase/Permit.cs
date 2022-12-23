// ReSharper disable ClassNeverInstantiated.Global

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Permit : IEntity
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
