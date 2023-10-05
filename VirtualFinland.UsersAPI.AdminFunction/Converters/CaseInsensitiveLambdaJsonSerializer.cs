using System.Text.Json;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace VirtualFinland.Converters;

public class CaseInsensitiveLambdaJsonSerializer : DefaultLambdaJsonSerializer
{
    /// <summary>
    /// Constructs instance of serializer.
    /// </summary>
    public CaseInsensitiveLambdaJsonSerializer()
        : base(ConfigureJsonSerializerOptions)
    {

    }

    private static void ConfigureJsonSerializerOptions(JsonSerializerOptions options)
    {
        options.PropertyNameCaseInsensitive = true;
    }
}