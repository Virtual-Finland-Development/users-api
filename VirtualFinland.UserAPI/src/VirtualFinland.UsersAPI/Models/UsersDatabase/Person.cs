using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Person : Auditable<Person>, IEntity
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

    public void SetupAuditEvents(UsersDbContext dbContext, RequestAuthenticatedUser requestAuthenticatedUser)
    {
        if (!dbContext.ChangeTracker.HasChanges()) return;

        SetupAuditEvent(dbContext.Entry(this)?.State, requestAuthenticatedUser);
        AdditionalInformation?.SetupAuditEvent(dbContext.Entry(AdditionalInformation)?.State, requestAuthenticatedUser);
        WorkPreferences?.SetupAuditEvent(dbContext.Entry(WorkPreferences)?.State, requestAuthenticatedUser);

        foreach (var entry in Occupations)
        {
            entry.SetupAuditEvent(dbContext.Entry(entry)?.State, requestAuthenticatedUser);
        }

        foreach (var entry in Educations)
        {
            entry.SetupAuditEvent(dbContext.Entry(entry)?.State, requestAuthenticatedUser);
        }

        foreach (var entry in Certifications)
        {
            entry.SetupAuditEvent(dbContext.Entry(entry)?.State, requestAuthenticatedUser);
        }

        foreach (var entry in Permits)
        {
            entry.SetupAuditEvent(dbContext.Entry(entry)?.State, requestAuthenticatedUser);
        }

        foreach (var entry in Skills)
        {
            entry.SetupAuditEvent(dbContext.Entry(entry)?.State, requestAuthenticatedUser);
        }

        foreach (var entry in LanguageSkills)
        {
            entry.SetupAuditEvent(dbContext.Entry(entry)?.State, requestAuthenticatedUser);
        }
    }
}
