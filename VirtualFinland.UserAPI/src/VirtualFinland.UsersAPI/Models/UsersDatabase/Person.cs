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
    public DateTime? LastActive { get; set; } // Last time the user accessed the system
    public bool ToBeDeletedFromInactivity { get; set; }

    // Relationships
    public PersonAdditionalInformation? AdditionalInformation { get; set; } = null!;
    public WorkPreferences? WorkPreferences { get; set; } = null!;

    public ICollection<Occupation> Occupations { get; set; } = null!;
    public ICollection<Education> Educations { get; set; } = null!;
    public ICollection<Certification> Certifications { get; set; } = null!;
    public ICollection<Permit> Permits { get; set; } = null!;
    public ICollection<Skills> Skills { get; set; } = null!;
    public ICollection<Language> LanguageSkills { get; set; } = null!;

    public List<PersonTermsOfServiceAgreement> TermsOfServiceAgreements { get; } = new();

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
