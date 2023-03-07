
namespace VirtualFinland.UserAPI.Models.Shared;

public class AuthorizeResponseAuthorization
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int ExpiresAt { get; set; }
    public int IssuedAt { get; set; }
    public string Issuer { get; set; } = string.Empty;
}
