using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Models.Shared;

/// <summary>
///     Region code based on ISO 3166-2:FI standard
///     https://fi.wikipedia.org/wiki/ISO_3166-2:FI
/// </summary>
[JsonConverter(typeof(JsonEnumMemberValueConverterFactory))]
public enum Region
{
    [EnumMember(Value = "FI-01")] Ahvenanmaa = 1,
    [EnumMember(Value = "FI-02")] EtelaKarjala = 2,
    [EnumMember(Value = "FI-03")] EtelaPohjanmaa = 3,
    [EnumMember(Value = "FI-04")] EtelaSavo = 4,
    [EnumMember(Value = "FI-05")] Kainuu = 5,
    [EnumMember(Value = "FI-06")] KantaHame = 6,
    [EnumMember(Value = "FI-07")] KeskiPohjanmaa = 7,
    [EnumMember(Value = "FI-08")] KeskiSuomi = 8,
    [EnumMember(Value = "FI-09")] Kymenlaakso = 9,
    [EnumMember(Value = "FI-10")] Lappi = 10,
    [EnumMember(Value = "FI-11")] Pirkanmaa = 11,
    [EnumMember(Value = "FI-12")] Pohjanmaa = 12,
    [EnumMember(Value = "FI-13")] PohjoisKarjala = 13,
    [EnumMember(Value = "FI-14")] PohjoisPohjanmaa = 14,
    [EnumMember(Value = "FI-15")] PohjoisSavo = 15,
    [EnumMember(Value = "FI-16")] PaijatHame = 16,
    [EnumMember(Value = "FI-17")] Satakunta = 17,
    [EnumMember(Value = "FI-18")] Uusimaa = 18,
    [EnumMember(Value = "FI-19")] VarsinaisSuomi = 19
}
