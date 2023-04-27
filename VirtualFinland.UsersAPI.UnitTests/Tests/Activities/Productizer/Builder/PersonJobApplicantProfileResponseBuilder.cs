using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;

internal class PersonJobApplicantProfileResponseBuilder
{
    private readonly List<string> _permits = new();
    private List<PersonJobApplicantProfileResponse.Certification> _certifications = new();
    private List<PersonJobApplicantProfileResponse.Education> _educations = new();
    private List<PersonJobApplicantProfileResponse.LanguageSkill> _languageSkills = new();
    private List<PersonJobApplicantProfileResponse.Occupation> _occupations = new();
    private List<PersonJobApplicantProfileResponse.OtherSkill> _otherSkills = new();

    private PersonJobApplicantProfileResponse.WorkPreferences _workPreferences =
        new PersonJobApplicantProfileResponseWorkPreferencesBuilder().Build();

    public PersonJobApplicantProfileResponseBuilder WithCertifications(
        List<PersonJobApplicantProfileResponse.Certification> value)
    {
        _certifications = value;
        return this;
    }

    public PersonJobApplicantProfileResponseBuilder WithEducations(
        List<PersonJobApplicantProfileResponse.Education> value)
    {
        _educations = value;
        return this;
    }

    public PersonJobApplicantProfileResponseBuilder WithLanguageSkills(
        List<PersonJobApplicantProfileResponse.LanguageSkill> value)
    {
        _languageSkills = value;
        return this;
    }

    public PersonJobApplicantProfileResponseBuilder WithOccupations(
        List<PersonJobApplicantProfileResponse.Occupation> value)
    {
        _occupations = value;
        return this;
    }

    public PersonJobApplicantProfileResponseBuilder WithOtherSkills(
        List<PersonJobApplicantProfileResponse.OtherSkill> value)
    {
        _otherSkills = value;
        return this;
    }

    public PersonJobApplicantProfileResponseBuilder WithWorkPreferences(
        PersonJobApplicantProfileResponse.WorkPreferences value)
    {
        _workPreferences = value;
        return this;
    }

    public PersonJobApplicantProfileResponse Build()
    {
        return new PersonJobApplicantProfileResponse
        {
            Occupations = _occupations,
            Educations = _educations,
            LanguageSkills = _languageSkills,
            OtherSkills = _otherSkills,
            Certifications = _certifications,
            Permits = _permits,
            workPreferences = _workPreferences
        };
    }
}
