using Bogus;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

public class PersonAdditionalInformationBuilder
{
    private static readonly Faker Faker = new();

    private readonly Address _address = new()
    {
        City = Faker.Address.City(),
        StreetAddress = Faker.Address.StreetAddress(),
        Country = Faker.Address.Country(),
        ZipCode = Faker.Address.ZipCode()
    };

    private readonly string _citizenshipCode = "FR";
    private readonly string _countryOfBirthCode = "FR";
    private readonly DateTime _created = DateTime.UtcNow;
    private readonly DateOnly _dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow);
    private readonly string _gender = "Male";
    private readonly DateTime _modified = DateTime.UtcNow;
    private readonly string _nativeLanguageCode = "FR";
    private readonly string _occupationCode = "4012";

    private Guid _id = Guid.Empty;

    public PersonAdditionalInformationBuilder WithId(Guid value)
    {
        _id = value;
        return this;
    }

    public PersonAdditionalInformation Build()
    {
        return new PersonAdditionalInformation
        {
            Id = _id,
            Address = _address,
            Created = _created,
            Modified = _modified,
            CitizenshipCode = _citizenshipCode,
            CountryOfBirthCode = _countryOfBirthCode,
            OccupationCode = _occupationCode,
            NativeLanguageCode = _nativeLanguageCode,
            Gender = _gender,
            DateOfBirth = _dateOfBirth
        };
    }
}
