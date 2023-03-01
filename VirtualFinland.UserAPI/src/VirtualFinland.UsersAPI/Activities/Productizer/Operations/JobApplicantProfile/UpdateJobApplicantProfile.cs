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

            person.WorkPreferences ??= new WorkPreferences();

            person.WorkPreferences.PreferredRegionCode = new List<string>();
            foreach (var region in command.WorkPreferences.PreferredRegion)
            {
                person.WorkPreferences.PreferredRegionCode.Add(region);
            }

            person.WorkPreferences.PreferredMunicipalityCode = new List<string>();
            foreach (var municipality in command.WorkPreferences.PreferredMunicipality)
            {
                person.WorkPreferences.PreferredMunicipalityCode.Add(municipality);
            }

            person.WorkPreferences.EmploymentTypeCode = command.WorkPreferences.TypeOfEmployment;
            person.WorkPreferences.WorkingTimeCode = command.WorkPreferences.WorkingTime;

            person.WorkPreferences.WorkingLanguageEnum = new List<string>();
            foreach (var workingLanguage in command.WorkPreferences.WorkingLanguage)
            {
                person.WorkPreferences.WorkingLanguageEnum.Add(workingLanguage);
            }

            person.WorkPreferences.NaceCode = command.WorkPreferences.NaceCode;
            person.Permits = command.Permits.Select(x => new Permit { TypeCode = x }).ToList();

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
            }


            return new Response
            {
                Occupations = person.Occupations.Select(x => new Request.Occupation
                {
                    EscoIdentifier = x.EscoUri,
                    EscoCode = x.EscoCode,
                    WorkExperience = x.WorkMonths
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
                Permits = (from p in person.Permits where p.TypeCode is not null select p.TypeCode).ToList(),
                WorkPreferences = new Request.WorkPreferenceValues
                {
                    PreferredMunicipality = person.WorkPreferences.PreferredMunicipalityCode?.ToList() ?? new List<string>(),
                    PreferredRegion = person.WorkPreferences.PreferredRegionCode?.ToList() ?? new List<string>(),
                    WorkingTime = person.WorkPreferences.WorkingTimeCode,
                    TypeOfEmployment = person.WorkPreferences.EmploymentTypeCode,
                    WorkingLanguage = person.WorkPreferences.WorkingLanguageEnum?.ToList() ?? new List<string>(),
                    NaceCode = person.WorkPreferences.NaceCode
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
        public List<Occupation> Occupations { get; init; } = null!;
        public List<Education> Educations { get; init; } = null!;
        public List<LanguageSkill> LanguageSkills { get; init; } = null!;
        public List<OtherSkill> OtherSkills { get; init; } = null!;
        public List<Certification> Certifications { get; init; } = null!;
        public List<string> Permits { get; init; } = null!;
        public WorkPreferenceValues WorkPreferences { get; init; } = null!;

        public record Occupation
        {
            public string? EscoIdentifier { get; init; }
            public string? EscoCode { get; init; }
            public int? WorkExperience { get; init; }
        }

        public record Education
        {
            public string? EducationLevel { get; init; }
            public string? EducationField { get; init; }

            [JsonConverter(typeof(DateOnlyJsonConverter))]
            public DateOnly? GraduationDate { get; init; }
        }

        public record LanguageSkill
        {
            public string? EscoIdentifier { get; init; }
            public string? LanguageCode { get; init; }
            public string? SkillLevel { get; init; }
        }

        public record OtherSkill
        {
            public string? EscoIdentifier { get; init; }
            public string? SkillLevel { get; init; }
        }

        public record Certification
        {
            public string? CertificationName { get; init; }
            public string? QualificationType { get; init; }
        }

        public record WorkPreferenceValues
        {
            public List<string> PreferredRegion { get; init; } = null!;
            public List<string> PreferredMunicipality { get; init; } = null!;
            public string? TypeOfEmployment { get; init; }
            public string? WorkingTime { get; init; }
            public List<string> WorkingLanguage { get; init; } = null!;
            public string? NaceCode { get; init; }
        }
    }
}
