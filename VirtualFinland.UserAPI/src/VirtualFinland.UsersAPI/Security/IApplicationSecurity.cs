namespace VirtualFinland.UserAPI.Security.Models;

public interface IApplicationSecurity
{
    Task<AuthenticationCandinate> GetAuthenticationCandinate(string token);
    Task VerifyPersonTermsOfServiceAgreement(Guid personId, string audience);
}