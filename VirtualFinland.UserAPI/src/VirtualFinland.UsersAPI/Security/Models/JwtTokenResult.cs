namespace VirtualFinland.UserAPI.Security.Models;

public class JwtTokenResult
{
    public string UserId { get; set; } = default!;
    public string Issuer { get; set; } = default!;
}