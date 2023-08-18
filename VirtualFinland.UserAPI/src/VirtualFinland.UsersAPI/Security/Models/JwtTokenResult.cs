namespace VirtualFinland.UserAPI.Security.Models;

public class JwtTokenResult
{
    public string? UserId { get; set; }
    public string? Issuer { get; set; }
}