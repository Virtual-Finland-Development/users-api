using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}
