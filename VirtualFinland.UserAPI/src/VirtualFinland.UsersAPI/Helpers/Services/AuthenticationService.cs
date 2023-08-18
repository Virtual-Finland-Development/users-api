using VirtualFinland.UserAPI.Models.UsersDatabase;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AuthenticationService
{
    private readonly UserSecurityService _userSecurityService;

    public AuthenticationService(UserSecurityService userSecurityService)
    {
        _userSecurityService = userSecurityService;
    }

    public async Task<Guid?> GetCurrentUserId(HttpRequest httpRequest)
    {
        var person = await GetCurrentUser(httpRequest);
        return person?.Id;
    }

    public async Task<Person?> GetCurrentUser(HttpRequest httpRequest)
    {
        var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        var person = await _userSecurityService.VerifyAndGetAuthenticatedUser(token);
        return person;
    }

    public JwtTokenResult ParseAuthenticationHeader(HttpRequest httpRequest)
    {
        var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        return _userSecurityService.ParseJwtToken(token);
    }
}
