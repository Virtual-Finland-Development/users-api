// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Person : IEntity
{
    [MaxLength(255)]
    public string? GivenName { get; set; }
    
    [MaxLength(255)]
    public string? LastName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(3)]
    public string? ResidencyCode { get; set; }
    
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    
    // Relationships
    public virtual ICollection<Occupation>? Occupations { get; set; }
    public virtual ICollection<Education>? Educations { get; set; }
    public virtual ICollection<Certification>? Certifications { get; set; }
    public virtual WorkPreferences? WorkPreferences { get; set; }
    public virtual ICollection<Permit>? Permits { get; set; }
    public virtual ICollection<Skills>? Skills { get; set; }
    public virtual ICollection<Language>? LanguageSkills { get; set; }
}
