using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public PersonAdditionalInformation? AdditionalInformation { get; set; }
    public WorkPreferences? WorkPreferences { get; set; }

    public ICollection<Occupation>? Occupations { get; set; }
    public ICollection<Education>? Educations { get; set; }
    public ICollection<Certification>? Certifications { get; set; }
    public ICollection<Permit>? Permits { get; set; }
    public ICollection<Skills>? Skills { get; set; }
    public ICollection<Language>? LanguageSkills { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
