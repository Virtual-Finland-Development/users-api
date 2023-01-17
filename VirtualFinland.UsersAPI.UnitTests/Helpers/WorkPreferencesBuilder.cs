using VirtualFinland.UserAPI.Models.Shared;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UsersAPI.UnitTests.Helpers;

internal class WorkPreferencesBuilder
{
    private readonly DateTime _created = default;
    private readonly EmploymentType? _employmentTypeCode = EmploymentType.Temporary;
    private readonly Guid _id = Guid.Empty;
    private readonly DateTime _modified = default;
    private readonly ICollection<Municipality>? _municipalities = new List<Municipality> { Municipality.Lappeenranta };
    private readonly ICollection<Region>? _regions = new List<Region> { Region.Kainuu };
    private readonly string? _workingLanguage = "fi";
    private readonly WorkingTime? _workingTime = WorkingTime.DayShift;

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
