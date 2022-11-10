using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class User : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    [MaxLength(255)]
    public string? FirstName { get; set; }
    
    [MaxLength(255)]
    public string? LastName { get; set; }
    
    [MaxLength(512)]
    public string? Address { get; set; }
    
    public bool ImmigrationDataConsent { get; set; }
    
    public bool JobsDataConsent { get; set; }
    
    public DateOnly? DateOfBirth { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; }
    
    [MaxLength(10)]
    public string? CountryOfBirthCode { get; set; }
    
    [MaxLength(10)]
    public string? NativeLanguageCode { get; set; }
    
    [MaxLength(10)]
    public string? OccupationCode { get; set; }
    
    [MaxLength(10)]
    public string? CitizenshipCode { get; set; }
}