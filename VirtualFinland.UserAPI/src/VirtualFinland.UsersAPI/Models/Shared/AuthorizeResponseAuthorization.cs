
using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Shared;

public class AuthorizeResponseAuthorization
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("expiresAt")]
    public int ExpiresAt { get; set; }
    [JsonPropertyName("issuedAt")]
    public int IssuedAt { get; set; }
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;
}
