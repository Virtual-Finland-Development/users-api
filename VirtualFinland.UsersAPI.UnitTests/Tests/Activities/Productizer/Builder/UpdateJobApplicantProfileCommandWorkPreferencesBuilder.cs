using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;

internal class UpdateJobApplicantProfileCommandWorkPreferencesBuilder
{
    private readonly string? _employmentTypeCode = "permanent";
    private readonly List<string> _municipalities = new() { "091" };
    private readonly string _workingLanguage = "en";
    private readonly string? _workingTime = "03";
    private List<string> _regions = new() { "FI-06" };


    public UpdateJobApplicantProfileCommandWorkPreferencesBuilder WithRegions(List<string> value)
    {
        _regions = value;
        return this;
    }

    public UpdateJobApplicantProfile.Request.WorkPreferenceValues Build()
    {
        return new UpdateJobApplicantProfile.Request.WorkPreferenceValues
        {
            PreferredMunicipality = _municipalities,
            PreferredRegion = _regions,
            WorkingLanguage = _workingLanguage,
            WorkingTime = _workingTime,
            TypeOfEmployment = _employmentTypeCode
        };
    }
}
