using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

namespace VirtualFinland.UsersAPI.UnitTests.Tests.Activities.Productizer.Builder;

public class UpdateJobApplicantProfileCommandBuilder
{
    private readonly List<UpdateJobApplicantProfile.Request.Certification>
        _certifications = new()
        {
            new UpdateJobApplicantProfile.Request.Certification
            {
                CertificationName = "Hygiene passport",
                QualificationType = "Institutional"
            }
        };

    private readonly List<UpdateJobApplicantProfile.Request.Education>
        _educations = new()
        {
            new UpdateJobApplicantProfile.Request.Education
            {
                EducationField = "0731",
                EducationLevel = "7",
                GraduationDate = new DateOnly(2022, 01, 31)
            }
        };

    private readonly List<UpdateJobApplicantProfile.Request.Occupation>
        _occupations = new()
        {
            new UpdateJobApplicantProfile.Request.Occupation
            {
                EscoCode = "2654.1.7",
                EscoIdentifier = "http://data.europa.eu/esco/occupation/0022f466-426c-41a4-ac96-a235c945cf97",
                NaceCode = "62.01",
                WorkExperience = 1
            }
        };

    private readonly List<UpdateJobApplicantProfile.Request.LanguageSkill>
        _languageSkills = new()
        {
            new UpdateJobApplicantProfile.Request.LanguageSkill
            {
                EscoIdentifier = "http://data.europa.eu/esco/skill/6d3edede-8951-4621-a835-e04323300fa0",
                LanguageCode = "en",
                SkillLevel = "B2"
            }
        };

    private readonly List<UpdateJobApplicantProfile.Request.OtherSkill>
        _otherSkills = new()
        {
            new UpdateJobApplicantProfile.Request.OtherSkill
            {
                EscoIdentifier = "http://data.europa.eu/esco/skill/869fc2ce-478f-4420-8766-e1f02cec4fb2",
                SkillLevel = "beginner"
            }
        };

    private readonly List<string> _permits = new()
    {
        "42"
    };

    private readonly UpdateJobApplicantProfile.Request.WorkPreferenceValues
        _workPreferences = new()
        {
            PreferredMunicipality = new List<string> { "091" },
            PreferredRegion = new List<string> { "05" },
            WorkingLanguage = "en",
            WorkingTime = "03",
            TypeOfEmployment = "permanent"
        };

    public UpdateJobApplicantProfile.Command Build()
    {
        return new UpdateJobApplicantProfile.Command(
            _occupations,
            _educations,
            _languageSkills,
            _otherSkills,
            _certifications,
            _permits,
            _workPreferences
        );
    }
}