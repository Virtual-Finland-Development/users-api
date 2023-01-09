using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

public enum WorkingTime
{
    [JsonPropertyName("01")] DayShift = 1,
    [JsonPropertyName("02")] EveningShift = 2,
    [JsonPropertyName("03")] NightShift = 3,
    [JsonPropertyName("04")] WorkInEpisodes = 4,
    [JsonPropertyName("05")] FlexibleHours = 5,
    [JsonPropertyName("06")] NormalDays = 6,
    [JsonPropertyName("07")] WeekendHours = 7,
    [JsonPropertyName("08")] WorkInShifts = 8
}
