using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

/// <summary>
///     http://uri.suomi.fi/codelist/dataecon/employment
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmploymentType
{
    Permanent = 1,
    Temporary = 2,
    Seasonal = 3,
    SummerJob = 4
}
