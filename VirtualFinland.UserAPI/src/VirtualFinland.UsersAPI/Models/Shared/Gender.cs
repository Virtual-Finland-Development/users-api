using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    [JsonPropertyName("Other")] Other = 0,
    [JsonPropertyName("Male")] Male = 1,
    [JsonPropertyName("Female")] Female = 2
}
