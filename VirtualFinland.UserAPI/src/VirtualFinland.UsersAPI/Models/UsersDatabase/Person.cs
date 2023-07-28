using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Person : AuditableAndEncryptable, IEntity
{
    //[MaxLength(255)]
    [Encrypted]
    public string? GivenName { get; set; }

    //[MaxLength(255)]
    [Encrypted]
    public string? LastName { get; set; }

    [EmailAddress]
    [Encrypted]
    public string? Email { get; set; }

    [Phone]
    [Encrypted]
    public string? PhoneNumber { get; set; }

    //[MaxLength(3)]
    [Encrypted]
    public string? ResidencyCode { get; set; }

    // Relationships
    public PersonAdditionalInformation? AdditionalInformation { get; set; } = null!;
    public WorkPreferences? WorkPreferences { get; set; } = null!;

    public ICollection<Occupation> Occupations { get; set; } = null!;
    public ICollection<Education> Educations { get; set; } = null!;
    public ICollection<Certification> Certifications { get; set; } = null!;
    public ICollection<Permit> Permits { get; set; } = null!;
    public ICollection<Skills> Skills { get; set; } = null!;
    public ICollection<Language> LanguageSkills { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
