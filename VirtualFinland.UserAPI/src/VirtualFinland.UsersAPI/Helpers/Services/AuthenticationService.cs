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
        Guid? currentUserId = null;
        if (currentUserId == null)
        {
            var token = httpRequest.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
            var dbUser = await _userSecurityService.VerifyAndGetAuthenticatedUser(token);
            currentUserId = dbUser?.Id;
        }
        return currentUserId;
    }
}