// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Occupation : Auditable, IEntity
{
    [MaxLength(7)]
    public string? NaceCode { get; set; }
    
    [Url]
    public string? EscoUri { get; set; }
    
    [MaxLength(5)] // TODO: Check this
    public string? EscoCode { get; set; }
    
    // 50 years in same workplace should be plenty enough
    [Range(0, 600)]
    public int? WorkMonths { get; set; }
    
    public Guid Id { get; set; }

    
    /// <summary>
    /// Function used to update instance of an object with data from another instance
    /// </summary>
    /// <param name="instance"></param>
    public void Update(Occupation instance)
    {
        EscoCode = instance.EscoCode ?? EscoCode;
        EscoUri = instance.EscoUri ?? EscoUri;
        NaceCode = instance.NaceCode ?? NaceCode;
        WorkMonths = instance.WorkMonths ?? WorkMonths;
    }
}
