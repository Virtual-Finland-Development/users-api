using System.Text.Json;
using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkingLanguage
{
    Fi = 1,
    En = 2,
    Sv = 3
}
