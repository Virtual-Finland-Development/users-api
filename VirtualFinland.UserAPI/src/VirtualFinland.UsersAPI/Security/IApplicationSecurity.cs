namespace VirtualFinland.UserAPI.Security.Models;

public interface IApplicationSecurity
{
    Task<AuthenticatedUser> GetAuthenticatedUser(string token);
    Task VerifyPersonTermsOfServiceAgreement(Guid personId, string audience);
}