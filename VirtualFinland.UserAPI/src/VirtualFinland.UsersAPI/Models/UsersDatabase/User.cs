using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
    public string? StreetAddress { get; set; }
    
    [MaxLength(5)]
    public string? ZipCode { get; set; }
 
    [MaxLength(512)]
    public string? City { get; set; }
    
    [MaxLength(512)]
    public string? Country { get; set; }
    
    public bool ImmigrationDataConsent { get; set; }
    
    public bool JobsDataConsent { get; set; }
    
    public DateOnly? DateOfBirth { get; set; }
    
    public Gender Gender { get; set; }
    
    [MaxLength(10)]
    public string? CountryOfBirthCode { get; set; }
    
    [MaxLength(10)]
    public string? NativeLanguageCode { get; set; }
    
    [MaxLength(10)]
    public string? OccupationCode { get; set; }
    
    [MaxLength(10)]
    public string? CitizenshipCode { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    [JsonPropertyName("Male")]
    Male = 1,
    [JsonPropertyName("Female")]
    Female = 2
}
