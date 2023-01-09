// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Person : Auditable, IEntity
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

    // Relationships
    public virtual ICollection<Occupation>? Occupations { get; set; }
    public virtual ICollection<Education>? Educations { get; set; }
    public virtual ICollection<Certification>? Certifications { get; set; }
    public virtual WorkPreferences? WorkPreferences { get; set; }
    public virtual ICollection<Permit>? Permits { get; set; }
    public virtual ICollection<Skills>? Skills { get; set; }
    public virtual ICollection<Language>? LanguageSkills { get; set; }

    public Guid Id { get; set; }
}
