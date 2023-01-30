using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

public static class UpdateJobApplicantProfile
{
    public class Command : IRequest<Request>
    {
        public Command(
            List<Request.Occupation> occupations,
            List<Request.Education> educations,
            List<Request.LanguageSkill> languageSkills,
            List<Request.OtherSkill> otherSkills,
            List<Request.Certification> certifications,
            List<string> permits,
            Request.WorkPreferenceValues workPreferences)
        {
            Occupations = occupations;
            Educations = educations;
            LanguageSkills = languageSkills;
            OtherSkills = otherSkills;
            Certifications = certifications;
            Permits = permits;
            WorkPreferences = workPreferences;
        }

        public List<Request.Occupation> Occupations { get; }
        public List<Request.Education> Educations { get; }
        public List<Request.LanguageSkill> LanguageSkills { get; }
        public List<Request.OtherSkill> OtherSkills { get; }
        public List<Request.Certification> Certifications { get; }
        public List<string> Permits { get; }
        public Request.WorkPreferenceValues WorkPreferences { get; }


        [SwaggerIgnore]
        public Guid? UserId { get; set; }

        public void SetAuth(Guid? userDatabaseId)
        {
            UserId = userDatabaseId;
        }
    }

    public class Handler : IRequestHandler<Command, Request>
    {
        private readonly UsersDbContext _context;

        public Handler(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<Request> Handle(Command command, CancellationToken cancellationToken)
        {
            var person = await _context.Persons
                .Include(p => p.Occupations)
                .Include(p => p.Educations)
                .Include(p => p.LanguageSkills)
                .Include(p => p.Skills)
                .Include(p => p.Certifications)
                .Include(p => p.Permits)
                .Include(p => p.WorkPreferences)
                .FirstOrDefaultAsync(p => p.Id == command.UserId, cancellationToken);

            if (person is null) throw new NotFoundException();

            person.Occupations = command.Occupations.Select(x => new Occupation
            {
                EscoUri = x.EscoIdentifier,
                EscoCode = x.EscoCode,
                NaceCode = x.NaceCode,
                WorkMonths = x.WorkExperience
            }).ToList();

            person.Educations = command.Educations.Select(x => new Education
            {
                EducationLevelCode = x.EducationLevel,
                EducationFieldCode = x.EducationField,
                GraduationDate = x.GraduationDate
            }).ToList();

            person.LanguageSkills = command.LanguageSkills.Select(x => new Language
            {
                EscoUri = x.EscoIdentifier,
                LanguageCode = x.LanguageCode,
                CerfCode = x.SkillLevel
            }).ToList();

            person.Skills = command.OtherSkills.Select(x => new Skills
            {
                EscoUri = x.EscoIdentifier,
                SkillLevelEnum = x.SkillLevel
            }).ToList();

            person.Certifications = command.Certifications.Select(x => new Certification
            {
                Name = x.CertificationName,
                Type = x.QualificationType
            }).ToList();

            // TODO: Set navigation property as required so that he object is always loaded and don't have purkka below

            person.WorkPreferences ??= new WorkPreferences();
            foreach (var region in command.WorkPreferences.PreferredRegion)
            {
                person.WorkPreferences.PreferredRegionCode = new List<string>();
                person.WorkPreferences.PreferredRegionCode.Add(region);
            }

            foreach (var municipality in command.WorkPreferences.PreferredMunicipality)
            {
                person.WorkPreferences.PreferredMunicipalityCode = new List<string>();
                person.WorkPreferences.PreferredMunicipalityCode.Add(municipality);
            }

            person.WorkPreferences.EmploymentTypeCode = command.WorkPreferences.TypeOfEmployment;
            person.WorkPreferences.WorkingTimeCode = command.WorkPreferences.WorkingTime;
            person.WorkPreferences.WorkingLanguageEnum = command.WorkPreferences.WorkingLanguage;
            person.Permits = command.Permits.Select(x => new Permit { TypeCode = x }).ToList();


            await _context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Occupations = person.Occupations.Select(x => new Request.Occupation
                {
                    EscoIdentifier = x.EscoUri,
                    EscoCode = x.EscoCode,
                    NaceCode = x.NaceCode,
                    WorkExperience = (int)x.WorkMonths
                }).ToList(),
                Educations = person.Educations.Select(x => new Request.Education
                {
                    EducationField = x.EducationFieldCode,
                    EducationLevel = x.EducationLevelCode,
                    GraduationDate = x.GraduationDate
                }).ToList(),
                LanguageSkills = person.LanguageSkills.Select(x => new Request.LanguageSkill
                {
                    EscoIdentifier = x.EscoUri,
                    LanguageCode = x.LanguageCode,
                    SkillLevel = x.CerfCode
                }).ToList(),
                OtherSkills = person.Skills.Select(x => new Request.OtherSkill
                {
                    EscoIdentifier = x.EscoUri,
                    SkillLevel = x.SkillLevelEnum
                }).ToList(),
                Certifications = person.Certifications.Select(x => new Request.Certification
                {
                    QualificationType = x.Type,
                    CertificationName = x.Name
                }).ToList(),
                Permits = person.Permits.FirstOrDefault().TypeCode,
                WorkPreferences = new Request.WorkPreferenceValues
                {
                    PreferredMunicipality = person.WorkPreferences.PreferredMunicipalityCode.ToList(),
                    
                    PreferredRegion = person.WorkPreferences.PreferredRegionCode
                        .Select(RegionMapper.FromCodeSetToIso_3166_2)
                        .ToList(),
                    
                    WorkingTime = person.WorkPreferences.WorkingTimeCode,
                    TypeOfEmployment = person.WorkPreferences.EmploymentTypeCode,
                    WorkingLanguage = person.WorkPreferences.WorkingLanguageEnum
                }
            };
        }
    }

    public sealed record Response : Request
    {
    }


    [SwaggerSchema(Title = "UpdatePersonJobApplicantProfileRequest")]
    public record Request
    {
        public List<Occupation> Occupations { get; set; } = null!;
        public List<Education> Educations { get; set; } = null!;
        public List<LanguageSkill> LanguageSkills { get; set; } = null!;
        public List<OtherSkill> OtherSkills { get; set; } = null!;
        public List<Certification> Certifications { get; set; } = null!;
        public string Permits { get; set; } = null!;
        public WorkPreferenceValues WorkPreferences { get; set; } = null!;

        public record Occupation
        {
            public string? EscoIdentifier { get; init; }
            public string? EscoCode { get; set; }
            public string? NaceCode { get; set; }
            public int WorkExperience { get; set; }
        }

        public record Education
        {
            public string? EducationLevel { get; set; }
            public string? EducationField { get; set; }

            [JsonConverter(typeof(DateOnlyJsonConverter))]
            public DateOnly? GraduationDate { get; init; }
        }

        public record LanguageSkill
        {
            public string? EscoIdentifier { get; set; }
            public string? LanguageCode { get; set; }
            public string? SkillLevel { get; set; }
        }

        public record OtherSkill
        {
            public string? EscoIdentifier { get; set; }
            public string? SkillLevel { get; set; }
        }

        public record Certification
        {
            public string? CertificationName { get; set; }
            public string? QualificationType { get; set; }
        }

        public record WorkPreferenceValues
        {
            public List<string> PreferredRegion { get; set; } = null!;
            public List<string> PreferredMunicipality { get; set; } = null!;
            public string? TypeOfEmployment { get; set; }
            public string? WorkingTime { get; set; }
            public string WorkingLanguage { get; set; } = null!;
        }
    }
}
