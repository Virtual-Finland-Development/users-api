using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

internal class WorkPreferencesBuilder
{
    private readonly DateTime _created = default;
    private readonly string? _employmentTypeCode = "1";
    private readonly Guid _id = Guid.Empty;
    private readonly DateTime _modified = default;
    private readonly List<string>? _municipalities = new() { "405" };
    private readonly List<string>? _regions = new() { "05" };
    private readonly string? _workingLanguage = "fi";
    private readonly string? _workingTime = "1";

    public WorkPreferences Build()
    {
        return new WorkPreferences
        {
            Id = _id,
            PreferredRegionEnum = _regions,
            PreferredMunicipalityEnum = _municipalities,
            EmploymentTypeCode = _employmentTypeCode,
            WorkingTimeEnum = _workingTime,
            WorkingLanguageEnum = _workingLanguage,
            Created = _created,
            Modified = _modified
        };
    }
}
