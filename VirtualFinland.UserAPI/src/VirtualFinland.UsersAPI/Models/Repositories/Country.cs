using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Repositories;

public class Country
{
    [JsonPropertyName("twoLetterISORegionName")]
    public string? IsoCode { get; set; }

    [JsonPropertyName("threeLetterISORegionName")]
    public string? IsoCodeThreeLetter { get; set; }
}