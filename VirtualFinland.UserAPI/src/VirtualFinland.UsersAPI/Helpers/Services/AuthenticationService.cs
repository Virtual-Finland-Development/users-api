using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AuthenticationService
{
    private readonly UserSecurityService _userSecurityService;

    public AuthenticationService(UserSecurityService userSecurityService)
    {
        _userSecurityService = userSecurityService;
    }

    /// <summary>
    /// Authenticates and authorizes and returns the current user
    /// </summary>
    public async Task<AuthenticatedUser> GetCurrentUser(HttpRequest httpRequest, bool verifyTermsOfServiceAgreement = true)
    {
        var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        var authenticatedUser = await _userSecurityService.VerifyAndGetAuthenticatedUser(token);

        if (verifyTermsOfServiceAgreement) await _userSecurityService.VerifyPersonTermsOfServiceAgreement(authenticatedUser);

        return authenticatedUser;
    }

    /// <summary>
    /// Authenticates and authorizes the current user and returns the user id
    /// </summary>
    public async Task<Guid> GetCurrentUserId(HttpRequest httpRequest, bool verifyTermsOfServiceAgreement = true)
    {
        var currentUser = await GetCurrentUser(httpRequest, verifyTermsOfServiceAgreement);
        return currentUser.PersonId;
    }

    public Task<AuthenticatedUser> ParseAuthenticationHeader(HttpRequest httpRequest)
    {
        var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        return _userSecurityService.GetAuthenticatedUser(token);
    }
}
