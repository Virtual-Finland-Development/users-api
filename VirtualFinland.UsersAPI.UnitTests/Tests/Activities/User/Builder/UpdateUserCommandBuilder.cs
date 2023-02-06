using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Models.Shared;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.User.Builder;

public class UpdateUserCommandBuilder
{
    private Address? _address = new AddressBuilder().Build();
    private string _citizenshipCode = "fi";
    private string _countryOfBirthCode = "fi";
    private DateTime? _dateOfBirth = new(2022, 01, 01);
    private string _firstName = "FirstName";
    private string _gender = "Male";
    private List<string>? _jobTitles = new() { "programmer" };
    private string _lastName = "LastName";
    private string _nativeLanguageCode = "fin";
    private string _occupationCode = "01";
    private List<UpdateUser.UpdateUserRequestOccupation>? _occupations = new();
    private List<string>? _regions = new() { "Southern-Finland" };
    private UpdateUser.UpdateUserRequestWorkPreferences? _workPreferences;

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

    public UpdateUserCommandBuilder WithGender(string value)
    {
        _gender = value;
        return this;
    }

    public UpdateUserCommandBuilder WithDateOfBirth(DateTime? value)
    {
        _dateOfBirth = value;
        return this;
    }

    public UpdateUserCommandBuilder WithOccupations(IEnumerable<UpdateUser.UpdateUserRequestOccupation> value)
    {
        _occupations = value.ToList();
        return this;
    }

    public UpdateUserCommandBuilder WithWorkPreferences(UpdateUser.UpdateUserRequestWorkPreferences value)
    {
        _workPreferences = value;
        return this;
    }

    public UpdateUser.Command Build()
    {
        return new UpdateUser.Command(
            _firstName,
            _lastName,
            _address,
            _countryOfBirthCode,
            _nativeLanguageCode,
            _occupationCode,
            _citizenshipCode,
            _jobTitles,
            _regions,
            _gender,
            _dateOfBirth,
            _occupations,
            _workPreferences);
    }
}
