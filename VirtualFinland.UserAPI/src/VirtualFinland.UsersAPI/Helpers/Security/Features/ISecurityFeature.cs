using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace VirtualFinland.UserAPI.Helpers.Security.Features;

public interface ISecurityFeature
{
    void BuildAuthentication(AuthenticationBuilder authentication);
    void BuildAuthorization(AuthorizationOptions options);
    string GetSecurityPolicyScheme();
    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
}