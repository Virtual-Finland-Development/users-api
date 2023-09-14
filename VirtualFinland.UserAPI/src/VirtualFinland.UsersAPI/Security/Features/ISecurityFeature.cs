using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace VirtualFinland.UserAPI.Security.Features;

public interface ISecurityFeature
{
    void BuildAuthentication(AuthenticationBuilder authentication);
    void BuildAuthorization(AuthorizationOptions options);
    string GetSecurityPolicySchemeName();
    string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken);
    Task ValidateSecurityTokenAudience(string audience);

    public string Issuer { get; }
}