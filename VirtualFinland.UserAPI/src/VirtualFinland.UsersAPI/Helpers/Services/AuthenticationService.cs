using System.Security.Claims;
using VirtualFinland.UserAPI.Data;

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
            if (httpRequest.Path.Value != null &&
                httpRequest.HttpContext.User != null &&
                httpRequest.HttpContext.User.Identity != null &&
                httpRequest.HttpContext.User.Identity.IsAuthenticated &&
                !httpRequest.Path.Value.Contains("/identity"))
            {
                var dbUser = await _userSecurityService.VerifyAndGetAuthenticatedUser(httpRequest.HttpContext.User.Claims.First().Issuer, httpRequest.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? httpRequest.HttpContext.User.FindFirst(Constants.Web.ClaimNameId)?.Value);
                currentUserId = dbUser?.Id;
            }
        }
        return currentUserId;
    }
}