namespace VirtualFinland.UserAPI.Security.Models;

public interface IApplicationSecurity
{
    void BuildSecurity(WebApplicationBuilder builder);
    JwtTokenResult ParseJwtToken(string token);
}