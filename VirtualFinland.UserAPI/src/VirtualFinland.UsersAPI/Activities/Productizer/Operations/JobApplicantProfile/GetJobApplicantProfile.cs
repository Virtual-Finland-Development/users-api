using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Extensions;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

public static class GetJobApplicantProfile
{
    public class Query : AuthenticatedRequest<PersonJobApplicantProfileResponse>
    {
        public Query(RequestAuthenticatedUser RequestAuthenticatedUser) : base(RequestAuthenticatedUser)
        {
        }
    }

    public class Handler : IRequestHandler<Query, PersonJobApplicantProfileResponse>
    {
        private readonly UsersDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(UsersDbContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
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
                .SingleAsync(p => p.Id == request.User.PersonId, cancellationToken);

            _logger.LogAuditLogEvent(AuditLogEvent.Read, "JobApplicantProfile", request.User);

            return new PersonJobApplicantProfileResponse
            {
                Occupations = person.Occupations.Select(x =>
                    new PersonJobApplicantProfileResponse.Occupation(
                        x.EscoUri,
                        x.EscoCode,
                        x.WorkMonths ?? 0,
                        x.Employer
                    )).ToList(),

                Educations = person.Educations.Select(x => new PersonJobApplicantProfileResponse.Education
                {
                    EducationName = x.Name,
                    EducationField = x.EducationFieldCode,
                    EducationLevel = x.EducationLevelCode,
                    GraduationDate = x.GraduationDate ?? DateOnly.MinValue,
                    InstitutionName = x.InstitutionName
                }).ToList(),

                LanguageSkills = person.LanguageSkills.Select(x =>
                    new PersonJobApplicantProfileResponse.LanguageSkill(
                        x.EscoUri,
                        x.LanguageCode,
                        x.CerfCode?.ToString()
                    )).ToList(),

                OtherSkills = person.Skills.Select(x =>
                    new PersonJobApplicantProfileResponse.OtherSkill(
                        x.EscoUri,
                        x.SkillLevelEnum?.ToString()
                    )).ToList(),

                Certifications = person.Certifications.Select(x =>
                    new PersonJobApplicantProfileResponse.Certification(
                        x.Name,
                        x.EscoUri,
                        x.InstitutionName
                    )).ToList(),

                Permits = (from p in person.Permits where p.TypeCode is not null select p.TypeCode).ToList(),

                workPreferences = new PersonJobApplicantProfileResponse.WorkPreferences(
                    person.WorkPreferences?.NaceCode,
                    person.WorkPreferences?.PreferredRegionCode?.ToList() ?? new List<string>(),
                    person.WorkPreferences?.PreferredMunicipalityCode?.ToList() ?? new List<string>(),
                    person.WorkPreferences?.EmploymentTypeCode,
                    person.WorkPreferences?.WorkingTimeCode,
                    person.WorkPreferences?.WorkingLanguageEnum?.ToList() ?? new List<string>()
                )
            };
        }
    }
}

[SwaggerSchema(Title = "PersonJobApplicantProfileResponse")]
public record PersonJobApplicantProfileResponse
{
    public List<Occupation> Occupations { get; set; } = null!;
    public List<Education> Educations { get; set; } = null!;
    public List<LanguageSkill> LanguageSkills { get; set; } = null!;
    public List<OtherSkill> OtherSkills { get; set; } = null!;
    public List<Certification> Certifications { get; set; } = null!;
    public List<string> Permits { get; set; } = null!;
    public WorkPreferences workPreferences { get; set; } = null!;

    public record Occupation(
        string? EscoIdentifier,
        string? EscoCode,
        int? WorkExperience,
        string? Employer
    );

    public record Education
    {
        public string? EducationName { get; set; }
        public string? EducationLevel { get; set; }
        public string? EducationField { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? GraduationDate { get; set; }

        public string? InstitutionName { get; set; }
    }

    public record LanguageSkill(string? EscoIdentifier, string? LanguageCode, string? SkillLevel);

    public record OtherSkill(string? EscoIdentifier, string? SkillLevel);

    public record Certification(string? CertificationName, List<string>? EscoIdentifier, string? InstitutionName);

    public record WorkPreferences(
        string? NaceCode,
        List<string> PreferredRegion,
        List<string> PreferredMunicipality,
        string? TypeOfEmployment,
        string? WorkingTime,
        List<string> WorkingLanguage
    );
}
