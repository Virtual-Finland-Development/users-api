using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Security.Features;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Security;

public class ApplicationSecurity : IApplicationSecurity
{
    private readonly ITermsOfServiceRepository _termsOfServiceRepository;
    private readonly SecuritySetup _setup;
    public ApplicationSecurity(ITermsOfServiceRepository termsOfServiceRepository, SecuritySetup securitySetup)
    {
        _termsOfServiceRepository = termsOfServiceRepository;
        _setup = securitySetup;
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
        var securityFeature = _setup.Features.Find(o => o.Issuer == tokenIssuer) ?? throw new NotAuthorizedException("The given token issuer is not valid");

        // Resolve user id
        var userId = securityFeature.ResolveTokenUserId(parsedToken) ?? throw new NotAuthorizedException("The given token claim is not valid");
        return new JwtTokenResult { UserId = userId, Issuer = securityFeature.Issuer };
    }

    /// <summary>
    /// Verifies that the user has accepted the latest terms of service
    /// </summary>
    public async Task VerifyPersonTermsOfServiceAgreement(Guid personId)
    {
        if (!_setup.Options.TermsOfServiceAgreementRequired) return;
        // Fetch person terms of service agreement
        _ = await _termsOfServiceRepository.GetNewestTermsOfServiceAgreementByPersonId(personId) ?? throw new NotAuthorizedException("User has not accepted the latest terms of service.");
    }
}