using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Models.Shared;

/// <summary>
///     Region code based on https://koodistot.suomi.fi/codescheme;registryCode=jhs;schemeCode=maakunta_1_20220101
/// </summary>
[JsonConverter(typeof(JsonEnumMemberValueConverterFactory))]
public enum Region
{
    [EnumMember(Value = "21")] Ahvenanmaa = 21,
    [EnumMember(Value = "09")] EtelaKarjala = 9,
    [EnumMember(Value = "14")] EtelaPohjanmaa = 14,
    [EnumMember(Value = "10")] EtelaSavo = 10,
    [EnumMember(Value = "18")] Kainuu = 18,
    [EnumMember(Value = "05")] KantaHame = 5,
    [EnumMember(Value = "16")] KeskiPohjanmaa = 16,
    [EnumMember(Value = "13")] KeskiSuomi = 13,
    [EnumMember(Value = "08")] Kymenlaakso = 8,
    [EnumMember(Value = "19")] Lappi = 19,
    [EnumMember(Value = "06")] Pirkanmaa = 6,
    [EnumMember(Value = "15")] Pohjanmaa = 15,
    [EnumMember(Value = "12")] PohjoisKarjala = 12,
    [EnumMember(Value = "17")] PohjoisPohjanmaa = 17,
    [EnumMember(Value = "11")] PohjoisSavo = 11,
    [EnumMember(Value = "07")] PaijatHame = 7,
    [EnumMember(Value = "04")] Satakunta = 4,
    [EnumMember(Value = "01")] Uusimaa = 1,
    [EnumMember(Value = "02")] VarsinaisSuomi = 2
}
