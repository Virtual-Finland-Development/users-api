using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class PersonAdditionalInformation : Auditable, IEntity
{
    [Encrypted]
    public Address? Address { get; set; }
    [Encrypted]
    public string? DateOfBirth { get; set; }
    [Encrypted]
    public string? Gender { get; set; }

    //[MaxLength(10)]
    [Encrypted]
    public string? CountryOfBirthCode { get; set; }

    //[MaxLength(10)]
    [Encrypted]
    public string? NativeLanguageCode { get; set; }

    //[MaxLength(10)]
    [Encrypted]
    public string? OccupationCode { get; set; }

    //[MaxLength(10)]
    [Encrypted]
    public string? CitizenshipCode { get; set; }

    [JsonIgnore]
    public Person? Person { get; set; }

    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ForeignKey(nameof(Person))]
    public Guid Id { get; set; }

    [NotMapped]
    public string? EncryptionKey { get; set; }
}
