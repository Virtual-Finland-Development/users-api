using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models;

public class SearchProfile : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public Guid UserId { get; set; }
    
    public string? Name { get; set; }

    public string? JobTitles { get; set; }
    public string? Municipality { get; set; }
    public string? Regions { get; set; }

    public bool IsDefault { get; set; } = false;
}