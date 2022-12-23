// ReSharper disable ClassNeverInstantiated.Global

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Occupation : IEntity
{
    public string? NaceCode { get; set; }
    public string? EscoUri { get; set; }
    public string? EscoCode { get; set; }
    public int? WorkMonths { get; set; }
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
