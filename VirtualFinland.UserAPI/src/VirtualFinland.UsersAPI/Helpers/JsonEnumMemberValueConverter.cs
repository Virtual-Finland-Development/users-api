using System.Text.Json;
using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Helpers;

public sealed class JsonEnumMemberValueConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public JsonEnumMemberValueConverter(JsonSerializerOptions options)
    {
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();

        if (string.IsNullOrEmpty(enumString)) throw new JsonException();

        var enumValue = EnumUtilities.GetEnumValueOrDefault<T>(enumString);
        if (!enumValue.HasValue) throw new JsonException($"Could not get enum value for {enumString}");

        return enumValue.Value;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var enumOutput = EnumUtilities.GetEnumMemberValueOrDefault(value);
        if (string.IsNullOrEmpty(enumOutput)) throw new JsonException("Oops");

        writer.WriteStringValue(enumOutput);
    }
}
