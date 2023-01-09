using System.Runtime.Serialization;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public enum EmploymentType
{
    [EnumMember(Value = "permanent")] Permanent,
    [EnumMember(Value = "temporary")] Temporary,
    [EnumMember(Value = "seasonal")] Seasonal,
    [EnumMember(Value = "summerJob")] SummerJob
}
