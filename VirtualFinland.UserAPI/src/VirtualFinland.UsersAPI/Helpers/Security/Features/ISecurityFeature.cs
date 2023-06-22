using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace VirtualFinland.UserAPI.Helpers.Security.Features;

public interface ISecurityFeature
{
    void BuildAuthentication(AuthenticationBuilder authentication);
    void BuildAuthorization(AuthorizationOptions options);
    string GetSecurityPolicyScheme();
    string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken);

    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
}