using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Models.Shared;

/// <summary>
/// Region code based on ISO 3166-2:FI standard
/// https://fi.wikipedia.org/wiki/ISO_3166-2:FI
/// </summary>
[JsonConverter(typeof(JsonEnumMemberValueConverterFactory))]
public enum Region
{
    [EnumMember(Value = "01")] Ahvenanmaa = 1,
    [EnumMember(Value = "02")] EtelaKarjala = 2,
    [EnumMember(Value = "03")] EtelaPohjanmaa = 3,
    [EnumMember(Value = "04")] EtelaSavo = 4,
    [EnumMember(Value = "05")] Kainuu = 5,
    [EnumMember(Value = "06")] KantaHame = 6,
    [EnumMember(Value = "07")] KeskiPohjanmaa = 7,
    [EnumMember(Value = "08")] KeskiSuomi = 8,
    [EnumMember(Value = "09")] Kymenlaakso = 9,
    [EnumMember(Value = "10")] Lappi = 10,
    [EnumMember(Value = "11")] Pirkanmaa = 11,
    [EnumMember(Value = "12")] Pohjanmaa = 12,
    [EnumMember(Value = "13")] PohjoisKarjala = 13,
    [EnumMember(Value = "14")] PohjoisPohjanmaa = 14,
    [EnumMember(Value = "15")] PohjoisSavo = 15,
    [EnumMember(Value = "16")] PaijatHame = 16,
    [EnumMember(Value = "17")] Satakunta = 17,
    [EnumMember(Value = "18")] Uusimaa = 18,
    [EnumMember(Value = "19")] VarsinaisSuomi = 19
}
