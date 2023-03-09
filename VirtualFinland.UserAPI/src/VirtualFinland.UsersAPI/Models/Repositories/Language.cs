using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Repositories;


public record class Language
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}