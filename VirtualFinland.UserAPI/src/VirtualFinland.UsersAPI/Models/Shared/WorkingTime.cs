using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Models.Shared;

/// <summary>
///     http://uri.suomi.fi/codelist/dataecon/work_time
/// </summary>
[JsonConverter(typeof(JsonEnumMemberValueConverterFactory))]
public enum WorkingTime
{
    [EnumMember(Value = "01")] DayShift = 1,
    [EnumMember(Value = "02")] EveningShift = 2,
    [EnumMember(Value = "03")] NightShift = 3,
    [EnumMember(Value = "04")] WorkInEpisodes = 4,
    [EnumMember(Value = "05")] FlexibleHours = 5,
    [EnumMember(Value = "06")] NormalDays = 6,
    [EnumMember(Value = "07")] WeekendHours = 7,
    [EnumMember(Value = "08")] WorkInShifts = 8
}
