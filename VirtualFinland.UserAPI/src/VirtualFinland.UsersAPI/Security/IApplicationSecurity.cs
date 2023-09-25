namespace VirtualFinland.UserAPI.Security.Models;

public interface IApplicationSecurity
{
    Task<JwtTokenResult> ParseJwtToken(string token);
}