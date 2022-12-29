using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

public enum Region
{
    [JsonPropertyName("FI-01")] Fi01 = 1,
    [JsonPropertyName("FI-02")] Fi02 = 2,
    [JsonPropertyName("FI-03")] Fi03 = 3,
    [JsonPropertyName("FI-04")] Fi04 = 4,
    [JsonPropertyName("FI-05")] Fi05 = 5,
    [JsonPropertyName("FI-06")] Fi06 = 6,
    [JsonPropertyName("FI-07")] Fi07 = 7,
    [JsonPropertyName("FI-08")] Fi08 = 8,
    [JsonPropertyName("FI-09")] Fi09 = 9,
    [JsonPropertyName("FI-10")] Fi10 = 10,
    [JsonPropertyName("FI-11")] Fi11 = 11,
    [JsonPropertyName("FI-12")] Fi12 = 12,
    [JsonPropertyName("FI-13")] Fi13 = 13,
    [JsonPropertyName("FI-14")] Fi14 = 14,
    [JsonPropertyName("FI-15")] Fi15 = 15,
    [JsonPropertyName("FI-16")] Fi16 = 16,
    [JsonPropertyName("FI-17")] Fi17 = 17,
    [JsonPropertyName("FI-18")] Fi18 = 18,
    [JsonPropertyName("FI-19")] Fi19 = 19
}
