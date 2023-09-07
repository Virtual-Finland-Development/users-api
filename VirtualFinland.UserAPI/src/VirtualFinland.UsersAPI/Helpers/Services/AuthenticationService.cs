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
    /// Authenticates and authorizes the current user and returns the user id
    /// </summary>
    public async Task<Guid> GetCurrentUserId(HttpRequest httpRequest, bool verifyTermsOfServiceAgreement = true)
    {
        var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        var person = await _userSecurityService.VerifyAndGetAuthenticatedUser(token);

        if (verifyTermsOfServiceAgreement) await _userSecurityService.VerifyPersonTermsOfServiceAgreement(person.Id);

        return person.Id;
    }

    public JwtTokenResult ParseAuthenticationHeader(HttpRequest httpRequest)
    {
        var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        return _userSecurityService.ParseJwtToken(token);
    }
}
