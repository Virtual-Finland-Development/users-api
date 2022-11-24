using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer;

public class UpdateUserCommandBuilder
{
    private string _firstName = "FirstName";
    private string _lastName = "LastName";
    private Address? _address = new AddressBuilder().Build();
    private bool? _jobsDataConsent = true;
    private bool? _immigrationDataConsent = true;
    private string _countryOfBirthCode = "fi";
    private string _nativeLanguageCode = "fi-FI";
    private string _occupationCode = "01";
    private string _citizenshipCode = "fi";
    private List<string>? _jobTitles = new() { "programmer" };
    private List<string>? _regions = new() { "Southern-Finland" };
    private Gender _gender = Gender.Male;
    private DateTime? _dateOfBirth = new(2022, 01,01);

    public UpdateUserCommandBuilder WithFirstName(string value)
    {
        _firstName = value;
        return this;
    }

    public UpdateUserCommandBuilder WithLastName(string value)
    {
        _lastName = value;
        return this;
    }

    public UpdateUserCommandBuilder WithAddress(Address value)
    {
        _address = value;
        return this;
    }

    public UpdateUserCommandBuilder WithJobsDataConsent(bool? value)
    {
        _jobsDataConsent = value;
        return this;
    }

    public UpdateUserCommandBuilder WithImmigrationDataConsent(bool? value)
    {
        _immigrationDataConsent = value;
        return this;
    }

    public UpdateUserCommandBuilder WithCountryOfBirthCode(string value)
    {
        _countryOfBirthCode = value;
        return this;
    }

    public UpdateUserCommandBuilder WithNativeLanguageCode(string value)
    {
        _nativeLanguageCode = value;
        return this;
    }

    public UpdateUserCommandBuilder WithOccupationCode(string value)
    {
        _occupationCode = value;
        return this;
    }

    public UpdateUserCommandBuilder WithCitizenshipCode(string value)
    {
        _citizenshipCode = value;
        return this;
    }

    public UpdateUserCommandBuilder WithJobTitles(List<string>? value)
    {
        _jobTitles = value;
        return this;
    }

    public UpdateUserCommandBuilder WithRegions(List<string>? value)
    {
        _regions = value;
        return this;
    }

    public UpdateUserCommandBuilder WithGender(Gender value)
    {
        _gender = value;
        return this;
    }

    public UpdateUserCommandBuilder WithDateOfBirth(DateTime? value)
    {
        _dateOfBirth = value;
        return this;
    }

    public UpdateUser.Command Build()
    {
        return new UpdateUser.Command(
            _firstName,
            _lastName,
            _address,
            _jobsDataConsent,
            _immigrationDataConsent,
            _countryOfBirthCode,
            _nativeLanguageCode,
            _occupationCode,
            _citizenshipCode,
            _jobTitles,
            _regions,
            _gender,
            _dateOfBirth
        );
    }
}
