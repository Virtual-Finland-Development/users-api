using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;

public static class UpdateJobApplicantProfile
{
    public class Command : AuthenticatedRequest<Request>
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

        private sealed class WorkPreferencesValidator : AbstractValidator<Request.WorkPreferenceValues>
        {
            public WorkPreferencesValidator(ILanguageRepository languagesRepository)
            {
                RuleForEach(wp => wp.PreferredMunicipality)
                    .Must(x => EnumUtilities.TryParseWithMemberName<Municipality>(x, out _));

                RuleForEach(wp => wp.PreferredRegion)
                    .Must(x => EnumUtilities.TryParseWithMemberName<Region>(x, out _));

                RuleFor(x => x.TypeOfEmployment)
                    .Must(x => EnumUtilities.TryParseWithMemberName<EmploymentType>(x!, out _))
                    .When(x => !string.IsNullOrEmpty(x.TypeOfEmployment));

                RuleFor(x => x.WorkingTime)
                    .Must(x => EnumUtilities.TryParseWithMemberName<WorkingTime>(x!, out _))
                    .When(x => !string.IsNullOrEmpty(x.WorkingTime));

                var knownLanguages = languagesRepository.GetAllLanguages().Result;
                var knownLanguageCodes = knownLanguages.Select(x => x.TwoLetterISOLanguageName).ToList();
                RuleForEach(wp => wp.WorkingLanguage)
                    .Must(x =>
                    {
                        return !string.IsNullOrEmpty(x) && x.Length == 2 && knownLanguageCodes.Contains(x);
                    }).WithMessage("WorkingLanguage(s) are not valid");
            }
        }

        public sealed class OccupationsValidator : AbstractValidator<List<Request.Occupation>>
        {
            public OccupationsValidator(IOccupationsFlatRepository occupationsFlatRepository)
            {
                var knownOccupations = occupationsFlatRepository.GetAllOccupationsFlat().Result;

                RuleFor(occupations => occupations)
                    .Must((occupations, cancellationToken) =>
                    {
                        if (occupations is null || !occupations.Any()) return true;

                        return occupations.Any(x =>
                        {
                            var occupation = knownOccupations.FirstOrDefault(y => y.Notation == x.EscoCode);
                            return occupation != null;
                        });
                    }).WithMessage("EscoCode is not valid");
            }
        }

        public sealed class LanguageSkillsValidator : AbstractValidator<List<Request.LanguageSkill>>
        {
            public LanguageSkillsValidator(ILanguageRepository languagesRepository)
            {
                var knownLanguages = languagesRepository.GetAllLanguages().Result;
                var knownLanguageCodes = knownLanguages.Select(x => x.TwoLetterISOLanguageName).ToList();

                RuleFor(languageSkills => languageSkills)
                    .Must((languageSkills, cancellationToken) =>
                    {
                        if (languageSkills is null || !languageSkills.Any()) return true;

                        return languageSkills.Any(x =>
                        {
                            var language = knownLanguages.FirstOrDefault(y => y.TwoLetterISOLanguageName == x.LanguageCode);
                            return language != null;
                        });
                    }).WithMessage("LanguageSkills are not valid");
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(ILanguageRepository languagesRepository, IOccupationsFlatRepository occupationsFlatRepository)
            {
                RuleFor(command => command.User.PersonId).NotNull().NotEmpty();
                RuleFor(command => command.WorkPreferences).SetValidator(new WorkPreferencesValidator(languagesRepository));
                RuleFor(command => command.Occupations).SetValidator(new OccupationsValidator(occupationsFlatRepository));
                RuleFor(command => command.LanguageSkills).SetValidator(new LanguageSkillsValidator(languagesRepository));
            }
        }
    }

    public class Handler : IRequestHandler<Command, Request>
    {
        private readonly UsersDbContext _context;
        private readonly AnalyticsLogger<Handler> _logger;

        public Handler(UsersDbContext context, AnalyticsLoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateAnalyticsLogger<Handler>();
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
                .FirstOrDefaultAsync(p => p.Id == command.User.PersonId, cancellationToken) ?? throw new NotFoundException();

            person.Occupations = command.Occupations.Select(x => new Occupation
            {
                EscoUri = x.EscoIdentifier,
                EscoCode = x.EscoCode,
                WorkMonths = x.WorkExperience,
                Employer = x.Employer
            }).ToList();

            person.Educations = command.Educations.Select(x => new Education
            {
                Name = x.EducationName,
                EducationLevelCode = x.EducationLevel,
                EducationFieldCode = x.EducationField,
                GraduationDate = x.GraduationDate,
                InstitutionName = x.InstitutionName
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
                EscoUri = x.EscoIdentifier,
                InstitutionName = x.InstitutionName
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
                await _context.SaveChangesAsync(command.User, cancellationToken);
            }
            catch (DbUpdateException e)
            {
                throw new BadRequestException(e.InnerException?.Message ?? e.Message);
            }

            await _logger.LogAuditLogEvent(AuditLogEvent.Update, command.User);

            return new Response
            {
                Occupations = person.Occupations.Select(x => new Request.Occupation
                {
                    EscoIdentifier = x.EscoUri,
                    EscoCode = x.EscoCode,
                    WorkExperience = x.WorkMonths,
                    Employer = x.Employer
                }).ToList(),
                Educations = person.Educations.Select(x => new Request.Education
                {
                    EducationName = x.Name,
                    EducationField = x.EducationFieldCode,
                    EducationLevel = x.EducationLevelCode,
                    GraduationDate = x.GraduationDate,
                    InstitutionName = x.InstitutionName
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
                    CertificationName = x.Name,
                    EscoIdentifier = x.EscoUri,
                    InstitutionName = x.InstitutionName
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
            public string? Employer { get; set; }
        }

        public record Education
        {
            public string? EducationName { get; init; }
            public string? EducationLevel { get; init; }
            public string? EducationField { get; init; }

            [JsonConverter(typeof(DateOnlyJsonConverter))]
            public DateOnly? GraduationDate { get; init; }

            public string? InstitutionName { get; set; }
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
            public List<string>? EscoIdentifier { get; init; }
            public string? InstitutionName { get; init; }
        }

        public record WorkPreferenceValues
        {
            public string? NaceCode { get; init; }
            public List<string> PreferredRegion { get; init; } = null!;
            public List<string> PreferredMunicipality { get; init; } = null!;
            public string? TypeOfEmployment { get; init; }
            public string? WorkingTime { get; init; }
            public List<string> WorkingLanguage { get; init; } = null!;
        }
    }
}
