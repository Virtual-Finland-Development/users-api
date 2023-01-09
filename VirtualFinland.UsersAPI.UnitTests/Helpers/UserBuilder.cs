using Bogus;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class UserBuilder
{
    private static readonly Faker Faker = new();
    private readonly string _citizenshipCode = "FR";
    private readonly string _city = Faker.Address.City();
    private readonly string _country = Faker.Address.Country();
    private readonly string _countryOfBirthCode = "FR";
    private readonly DateTime _created = DateTime.UtcNow;
    private readonly DateOnly _dateOfBirth = DateOnly.FromDateTime(DateTime.Now);
    private readonly string _firstName = Faker.Person.FirstName;
    private readonly Gender _gender = Gender.Male;
    private readonly bool _immigrationDataConsent = false;
    private readonly bool _jobsDataConsent = true;
    private readonly string _lastName = Faker.Person.LastName;
    private readonly DateTime _modified = DateTime.UtcNow;
    private readonly string _nativeLanguageCode = "FR";
    private readonly string _occupationCode = "4012";
    private readonly string _streetAddress = Faker.Address.StreetAddress();
    private readonly string _zipCode = Faker.Address.ZipCode();
    private Guid _id = Guid.Empty;
    private List<Occupation> _occupations = new() { new OccupationsBuilder().Build() };
    private WorkPreferences? _workPreferences = new WorkPreferencesBuilder().Build();

    public UserBuilder WithId(Guid value)
    {
        _id = value;
        return this;
    }

    public UserBuilder WithOccupations(List<Occupation> value)
    {
        _occupations = value;
        return this;
    }

    public UserBuilder WithWorkPreferences(WorkPreferences value)
    {
        _workPreferences = value;
        return this;
    }

    public User Build()
    {
        return new User
        {
            Id = _id,
            StreetAddress = _streetAddress,
            ZipCode = _zipCode,
            City = _city,
            Country = _country,
            Created = _created,
            Modified = _modified,
            FirstName = _firstName,
            LastName = _lastName,
            JobsDataConsent = _jobsDataConsent,
            ImmigrationDataConsent = _immigrationDataConsent,
            CitizenshipCode = _citizenshipCode,
            CountryOfBirthCode = _countryOfBirthCode,
            OccupationCode = _occupationCode,
            NativeLanguageCode = _nativeLanguageCode,
            Gender = _gender,
            DateOfBirth = _dateOfBirth,
            Occupations = _occupations,
            WorkPreferences = _workPreferences
        };
    }
}
