namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Person : IEntity
{
    public string? GivenName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ResidencyCode { get; set; }

    public virtual ICollection<Occupation>? Occupations { get; set; }
    public virtual ICollection<Education>? Educations { get; set; }
    public virtual ICollection<Certification>? Certifications { get; set; }
    public virtual ICollection<WorkPreferences>? WorkPreferences { get; set; }
    public virtual ICollection<Permit>? Permits { get; set; }
    public virtual ICollection<Skills>? Skills { get; set; }

    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
