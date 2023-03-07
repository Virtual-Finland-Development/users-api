
using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

public class AuthorizeResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    [JsonPropertyName("authorization")]
    public AuthorizeResponseAuthorization Authorization { get; set; } = new AuthorizeResponseAuthorization();
    [JsonPropertyName("consent")]
    public AuthorizeResponseConsent? Consent { get; set; }
}