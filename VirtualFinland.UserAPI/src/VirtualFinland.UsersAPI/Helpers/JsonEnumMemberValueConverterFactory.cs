using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Helpers;

public class JsonEnumMemberValueConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        var converter =
            (JsonConverter)Activator.CreateInstance(
                typeof(JsonEnumMemberValueConverter<>).MakeGenericType(typeToConvert),
                flags,
                binder: null,
                args: new object?[] { options },
                culture: null
            )!;
        
        return converter;
    }
}
