using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class PersonAdditionalInformation : Auditable, IEntity
{
    public Address? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    [MaxLength(10)]
    public string? CountryOfBirthCode { get; set; }

    [MaxLength(10)]
    public string? NativeLanguageCode { get; set; }

    [MaxLength(10)]
    public string? OccupationCode { get; set; }

    [MaxLength(10)]
    public string? CitizenshipCode { get; set; }

    [JsonIgnore]
    public Person? Person { get; set; }

    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ForeignKey(nameof(Person))]
    public Guid Id { get; set; }
}
