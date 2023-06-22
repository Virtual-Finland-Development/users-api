namespace VirtualFinland.UserAPI.Helpers.Security;

public interface IApplicationSecuritySetup
{
    void BuildSecurity(WebApplicationBuilder builder);
    JwtTokenResult ParseJWTToken(string token);
}