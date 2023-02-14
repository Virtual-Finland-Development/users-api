using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

internal class WorkPreferencesBuilder
{
    private readonly DateTime _created = default;
    private readonly string? _employmentTypeCode = "2";
    private readonly Guid _id = Guid.Empty;
    private readonly DateTime _modified = default;
    private readonly List<string> _municipalities = new() { "405" };
    private readonly List<string> _regions = new() { "05" };
    private readonly List<string> _workingLanguage = new List<string>() { "fi" };
    private readonly string? _workingTime = "01";
    private readonly string? _naceCode = "42.323";

    public WorkPreferences Build()
    {
        return new WorkPreferences
        {
            Id = _id,
            PreferredRegionCode = _regions,
            PreferredMunicipalityCode = _municipalities,
            EmploymentTypeCode = _employmentTypeCode,
            WorkingTimeCode = _workingTime,
            WorkingLanguageEnum = _workingLanguage,
            NaceCode = _naceCode,
            Created = _created,
            Modified = _modified
        };
    }
}
