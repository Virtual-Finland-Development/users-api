namespace VirtualFinland.UserAPI.Security.Models;

public interface IApplicationSecurity
{
    Task<RequestAuthenticationCandinate> ParseJwtToken(string token);
    Task VerifyPersonTermsOfServiceAgreement(Guid personId);
}