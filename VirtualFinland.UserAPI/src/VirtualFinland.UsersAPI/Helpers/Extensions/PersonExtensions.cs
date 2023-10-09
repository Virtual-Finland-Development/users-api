using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Extensions;

public static class PersonExtensions
{

    /// <summary>
    ///  Helper method to setup request context related audit event data for a person related entities mutations logging.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="requestAuthenticatedUser"></param>
    /// <returns></returns>
    public static void SetupPersonAuditEvents(this Person person, UsersDbContext dbContext, RequestAuthenticatedUser requestAuthenticatedUser)
    {
        if (!dbContext.ChangeTracker.HasChanges()) return;

        person.SetupAuditEvent(dbContext, requestAuthenticatedUser);

        person.AdditionalInformation?.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        person.WorkPreferences?.SetupAuditEvent(dbContext, requestAuthenticatedUser);

        foreach (var entry in person.Occupations ?? Enumerable.Empty<Occupation>())
        {
            entry.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        }

        foreach (var entry in person.Occupations ?? Enumerable.Empty<Occupation>())
        {
            entry.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        }

        foreach (var entry in person.Educations ?? Enumerable.Empty<Education>())
        {
            entry.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        }

        foreach (var entry in person.Certifications ?? Enumerable.Empty<Certification>())
        {
            entry.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        }

        foreach (var entry in person.Permits ?? Enumerable.Empty<Permit>())
        {
            entry.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        }

        foreach (var entry in person.Skills ?? Enumerable.Empty<Skills>())
        {
            entry.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        }

        foreach (var entry in person.LanguageSkills ?? Enumerable.Empty<Language>())
        {
            entry.SetupAuditEvent(dbContext, requestAuthenticatedUser);
        }
    }
}