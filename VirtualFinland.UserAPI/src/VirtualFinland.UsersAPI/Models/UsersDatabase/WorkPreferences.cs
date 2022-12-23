// ReSharper disable ClassNeverInstantiated.Global

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class WorkPreferences
{
    // ReSharper disable once MemberCanBePrivate.Global
    public enum Municipality
    {
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public enum Region
    {
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public enum WorkingTime
    {
    }

    public Region? PreferredRegionEnum { get; set; }
    public Municipality? PreferredMunicipalityEnum { get; set; }
    public string? EmploymentTypeCode { get; set; }
    public WorkingTime? WorkingTimeEnum { get; set; }
    public string? WorkingLanguageEnum { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
