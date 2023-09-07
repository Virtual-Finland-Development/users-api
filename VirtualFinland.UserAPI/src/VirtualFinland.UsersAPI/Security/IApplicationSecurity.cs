namespace VirtualFinland.UserAPI.Security.Models;

public interface IApplicationSecurity
{
    JwtTokenResult ParseJwtToken(string token);
    Task VerifyPersonTermsOfServiceAgreement(Guid personId);
}