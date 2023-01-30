using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Swagger;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

public static class GetPersonJobApplicantProfile
{
    public class Query : IRequest<PersonJobApplicantProfileResponse>
    {
        public Query(Guid? personId)
        {
            PersonId = personId;
        }

        [SwaggerIgnore]
        public Guid? PersonId { get; }
    }

    public class Handler : IRequestHandler<Query, PersonJobApplicantProfileResponse>
    {
        private readonly UsersDbContext _context;

        public Handler(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<PersonJobApplicantProfileResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var person = await _context.Persons
                .Include(p => p.Occupations)
                .Include(p => p.Educations)
                .Include(p => p.LanguageSkills)
                .Include(p => p.Skills)
                .Include(p => p.Certifications)
                .Include(p => p.Permits)
                .Include(p => p.WorkPreferences)
                .SingleAsync(p => p.Id == request.PersonId, cancellationToken);

            return new PersonJobApplicantProfileResponse
            {
                Occupations = person.Occupations?.Select(x =>
                    new PersonJobApplicantProfileResponse.Occupation(
                        x.EscoCode,
                        x.EscoUri,
                        x.NaceCode,
                        x.WorkMonths ?? 0
                    )).ToList(),

                /*
                Educations = person.Educations?.Select(x =>
                    new PersonJobApplicantProfileResponse.Education(){}
                        x.EducationFieldCode,
                        x.EducationLevelCode,
                        x.GraduationDate.ToString()
                    )).ToList(),
                */

                Educations = person.Educations?.Select(x => new PersonJobApplicantProfileResponse.Education
                {
                    EducationField = x.EducationFieldCode,
                    EducationLevel = x.EducationLevelCode,
                    GraduationDate = x.GraduationDate ?? DateOnly.MinValue
                }).ToList(),

                LanguageSkills = person.LanguageSkills?.Select(x =>
                    new PersonJobApplicantProfileResponse.LanguageSkill(
                        x.EscoUri,
                        x.LanguageCode,
                        x.CerfCode.ToString()
                    )).ToList(),

                OtherSkills = person.Skills?.Select(x =>
                    new PersonJobApplicantProfileResponse.OtherSkill(
                        x.EscoUri,
                        x.SkillLevelEnum.ToString()
                    )).ToList(),

                Certifications = person.Certifications?.Select(x =>
                    new PersonJobApplicantProfileResponse.Certification(
                        x.Name,
                        x.Type
                    )).ToList(),

                permits = string.Join(",", person.Permits?.Select(x => x.TypeCode).ToList()),

                workPreferences = new PersonJobApplicantProfileResponse.WorkPreferences(
                    person.WorkPreferences?.PreferredMunicipalityCode?.ToList(),
                    person.WorkPreferences?.PreferredRegionCode?.ToList(),
                    person.WorkPreferences?.WorkingLanguageEnum,
                    person.WorkPreferences?.WorkingTimeCode,
                    person.WorkPreferences?.EmploymentTypeCode)
            };
        }
    }
}

[SwaggerSchema(Title = "PersonJobApplicantProfileResponse")]
public record PersonJobApplicantProfileResponse
{
    public List<Occupation> Occupations { get; set; }
    public List<Education> Educations { get; set; }
    public List<LanguageSkill> LanguageSkills { get; set; }
    public List<OtherSkill> OtherSkills { get; set; }
    public List<Certification> Certifications { get; set; }
    public string permits { get; set; }
    public WorkPreferences workPreferences { get; set; }

    public record Occupation(string EscoIdentifier, string EscoCode, string NaceCode, int WorkExperience);

    public record Education
    {
        public string EducationLevel { get; set; }
        public string EducationField { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? GraduationDate { get; set; }
    }

    public record LanguageSkill(string EscoIdentifier, string LanguageCode, string SkillLevel);

    public record OtherSkill(string EscoIdentifier, string SkillLevel);

    public record Certification(string CertificationName, string QualificationType);

    public record WorkPreferences(
        List<string> PreferredMunicipality,
        List<string> PreferredRegion,
        string WorkingLanguage,
        string WorkingTime,
        string TypeOfEmployment
    );
}
