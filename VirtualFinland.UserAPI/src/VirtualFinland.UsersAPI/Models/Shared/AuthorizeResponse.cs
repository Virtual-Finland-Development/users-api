
namespace VirtualFinland.UserAPI.Models.Shared;

public class AuthorizeResponse
{
    public string Message { get; set; } = string.Empty;
    public AuthorizeResponseAuthorization Authorization { get; set; } = new AuthorizeResponseAuthorization();
    public AuthorizeResponseConsent? Consent { get; set; }
}