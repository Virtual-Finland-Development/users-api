using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

internal class WorkPreferencesBuilder
{
    private readonly DateTime _created = default;
    private readonly EmploymentType? _employmentTypeCode = EmploymentType.Temporary;
    private readonly Guid _id = Guid.Empty;
    private readonly DateTime _modified = default;
    private readonly List<Municipality>? _municipalities = new() { Municipality.Lappeenranta };
    private readonly List<Region>? _regions = new() { Region.KantaHame };
    private readonly WorkingLanguage? _workingLanguage = WorkingLanguage.Fi;
    private readonly WorkingTime? _workingTime = WorkingTime.DayShift;

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
            Created = _created,
            Modified = _modified
        };
    }
}
