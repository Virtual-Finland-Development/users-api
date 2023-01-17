using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Other = 0,
    Male = 1,
    Female = 2
}
