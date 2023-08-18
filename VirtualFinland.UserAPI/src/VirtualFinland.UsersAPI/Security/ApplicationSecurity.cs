using System.IdentityModel.Tokens.Jwt;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security;

public class ApplicationSecurity : IApplicationSecurity
{
    private readonly List<ISecurityFeature> _features;

    public ApplicationSecurity(List<ISecurityFeature> features)
    {
        _features = features;
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id
    /// </summary>
    public JwtTokenResult ParseJwtToken(string token)
    {
        if (string.IsNullOrEmpty(token)) throw new NotAuthorizedException("No token provided");

        // Parse token
        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token)) throw new NotAuthorizedException("The given token is not valid");
        var parsedToken = tokenHandler.ReadJwtToken(token);

        // Resolve the security feature by token issuer (must be enabled) // @TODO: ensure the security feature is loaded before this
        var tokenIssuer = parsedToken.Issuer;
        var securityFeature = _features.Find(o => o.Issuer == tokenIssuer) ?? throw new NotAuthorizedException("The given token issuer is not valid");

        // Resolve user id
        var userId = securityFeature.ResolveTokenUserId(parsedToken) ?? throw new NotAuthorizedException("The given token claim is not valid");

        return new JwtTokenResult { UserId = userId, Issuer = tokenIssuer };
    }
}