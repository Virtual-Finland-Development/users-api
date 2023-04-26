using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

namespace VirtualFinland.UserAPI.Activities.Productizer;

public static class ProductizerProfileValidator
{
    public static bool IsPersonBasicInformationCreated(
        GetPersonBasicInformation.GetPersonBasicInformationResponse response)
    {
        return !string.IsNullOrEmpty(response.Email);
    }

    public static bool IsJobApplicantProfileCreated(PersonJobApplicantProfileResponse response)
    {
        if (response.Certifications is { Count: > 0 } ||
            response.Educations is { Count: > 0 } ||
            response.Occupations is { Count: > 0 } ||
            response.Permits is { Count: > 0 } ||
            response.LanguageSkills is { Count: > 0 } ||
            response.OtherSkills is { Count: > 0 })
            return true;

        if (response.workPreferences.PreferredMunicipality is { Count: > 0 } ||
            response.workPreferences.PreferredRegion is { Count: > 0 } ||
            response.workPreferences.WorkingLanguage is { Count: > 0 })
            return true;

        if (!string.IsNullOrEmpty(response.workPreferences.WorkingTime) ||
            !string.IsNullOrEmpty(response.workPreferences.NaceCode) ||
            !string.IsNullOrEmpty(response.workPreferences.TypeOfEmployment))
            return true;

        return false;
    }
}
