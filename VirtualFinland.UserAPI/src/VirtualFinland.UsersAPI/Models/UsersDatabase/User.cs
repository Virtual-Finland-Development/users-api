using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class User : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
    
    public string? Address { get; set; }
    
    public bool ImmigrationDataConsent { get; set; }
    
    public bool JobsDataConsent { get; set; }
    
    public DateOnly? DateOfBirth { get; set; }
    
    public string? Gender { get; set; }

    public string? CountryOfBirthISOCode { get; set; }

    public string? NativeLanguageISOCode { get; set; }

    public string? OccupationISCOCode { get; set; }

    public string? NationalityISOCode { get; set; }
}