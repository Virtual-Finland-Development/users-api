using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

[JsonConverter(typeof(JsonEnumMemberValueConverterFactory))]
public enum WorkingLanguage
{
    [EnumMember(Value = "fi")] Fi = 1,
    [EnumMember(Value = "en")] En = 2,
    [EnumMember(Value = "sv")] Sv = 3
}
