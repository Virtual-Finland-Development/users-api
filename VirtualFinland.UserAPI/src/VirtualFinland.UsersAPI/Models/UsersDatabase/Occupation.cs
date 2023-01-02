// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Occupation : Auditable, IEntity
{
    [MaxLength(7)]
    public string? NaceCode { get; set; }
    
    [Url]
    public string? EscoUri { get; set; }
    
    [MaxLength(16)]
    public string? EscoCode { get; set; }
    
    // 50 years in same workplace should be plenty enough
    [Range(0, 600)]
    public int? WorkMonths { get; set; }
    
    public Guid Id { get; set; }
}
