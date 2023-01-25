using Bogus;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using Person = VirtualFinland.UserAPI.Models.UsersDatabase.Person;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class PersonBuilder
{
    private static readonly Faker Faker = new();

    private readonly PersonAdditionalInformation _additionalInformation =
        new PersonAdditionalInformationBuilder().Build();

    private readonly ICollection<Certification>? _certifications = new List<Certification>();
    private readonly ICollection<Education>? _educations = new List<Education>();
    private readonly string? _email = Faker.Person.Email;
    private readonly string _firstName = Faker.Person.FirstName;
    private readonly ICollection<Language>? _languageSkills = new List<Language>();
    private readonly string _lastName = Faker.Person.LastName;
    private readonly ICollection<Permit>? _permits = new List<Permit>();
    private readonly string? _phoneNumber = Faker.Person.Phone;
    private readonly string? _residencyCode = "FR";
    private readonly ICollection<Skills>? _skills = new List<Skills>();
    private Guid _id = Guid.Empty;
    private List<Occupation> _occupations = new() { new OccupationsBuilder().Build() };
    private WorkPreferences? _workPreferences = new WorkPreferencesBuilder().Build();

    public PersonBuilder WithId(Guid value)
    {
        _id = value;
        return this;
    }

    public PersonBuilder WithOccupations(List<Occupation> value)
    {
        _occupations = value;
        return this;
    }

    public PersonBuilder WithWorkPreferences(WorkPreferences value)
    {
        _workPreferences = value;
        return this;
    }

    public Person Build()
    {
        return new Person
        {
            Id = _id,
            GivenName = _firstName,
            LastName = _lastName,
            Email = _email,
            PhoneNumber = _phoneNumber,
            ResidencyCode = _residencyCode,
            AdditionalInformation = _additionalInformation,
            WorkPreferences = _workPreferences,
            Occupations = _occupations,
            Educations = _educations,
            Certifications = _certifications,
            Permits = _permits,
            Skills = _skills,
            LanguageSkills = _languageSkills
        };
    }
}
