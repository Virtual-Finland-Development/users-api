namespace VirtualFinland.UserAPI.Helpers.Security;

public interface IApplicationSecurity
{
    void BuildSecurity(WebApplicationBuilder builder);
    JwtTokenResult ParseJwtToken(string token);
}