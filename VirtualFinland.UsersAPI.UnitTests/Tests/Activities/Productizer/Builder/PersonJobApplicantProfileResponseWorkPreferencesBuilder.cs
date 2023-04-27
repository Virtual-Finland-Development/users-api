using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;

internal class PersonJobApplicantProfileResponseWorkPreferencesBuilder
{
    private string? _naceCode = "42.323";
    private List<string> _preferredMunicipality = new() { "405" };
    private List<string> _preferredRegion = new() { "05" };
    private string? _typeOfEmployment = "2";
    private List<string> _workingLanguage = new() { "fi" };
    private string? _workingTime = "01";

    public PersonJobApplicantProfileResponseWorkPreferencesBuilder WithNaceCode(string? value)
    {
        _naceCode = value;
        return this;
    }

    public PersonJobApplicantProfileResponseWorkPreferencesBuilder WithPreferredMunicipalities(List<string> value)
    {
        _preferredMunicipality = value;
        return this;
    }

    public PersonJobApplicantProfileResponseWorkPreferencesBuilder WithPreferredRegions(List<string> value)
    {
        _preferredRegion = value;
        return this;
    }

    public PersonJobApplicantProfileResponseWorkPreferencesBuilder WithEmploymentType(string? value)
    {
        _typeOfEmployment = value;
        return this;
    }

    public PersonJobApplicantProfileResponseWorkPreferencesBuilder WithWorkingLanguage(List<string> value)
    {
        _workingLanguage = value;
        return this;
    }

    public PersonJobApplicantProfileResponseWorkPreferencesBuilder WithWorkingTime(string? value)
    {
        _workingTime = value;
        return this;
    }

    public PersonJobApplicantProfileResponse.WorkPreferences Build()
    {
        return new PersonJobApplicantProfileResponse.WorkPreferences(_naceCode, _preferredRegion,
            _preferredMunicipality, _typeOfEmployment, _workingTime, _workingLanguage);
    }
}
