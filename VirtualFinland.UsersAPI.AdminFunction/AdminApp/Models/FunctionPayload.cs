using System.Text.Json.Serialization;

namespace VirtualFinland.AdminFunction.AdminApp.Models;
public record FunctionPayload
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Actions Action { get; init; }
    public string? Data { get; init; }
}