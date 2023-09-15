using System.IdentityModel.Tokens.Jwt;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
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
    /// Parses the JWT token and returns the issuer and the user identity id, and the audience
    /// </summary>
    public async Task<AuthenticationCandinate> GetAuthenticationCandinate(string token)
    {
        if (string.IsNullOrEmpty(token)) throw new NotAuthorizedException("No token provided");

        // Parse token
        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token)) throw new NotAuthorizedException("The given token is not valid");
        var parsedToken = tokenHandler.ReadJwtToken(token);

        // Resolve the security feature by token issuer (must be enabled) // @TODO: ensure the security feature is loaded before this
        var tokenIssuer = parsedToken.Issuer;
        var securityFeature = _setup.Features.Find(o => o.Issuer == tokenIssuer) ?? throw new NotAuthorizedException("The given token issuer is not valid");

        // Resolve and validate the token audience
        var tokenAudience = parsedToken.Audiences.FirstOrDefault() ?? throw new NotAuthorizedException("The given token audience is not valid");
        await securityFeature.ValidateSecurityTokenAudience(tokenAudience);

        // Resolve user id
        var userId = securityFeature.ResolveTokenUserId(parsedToken) ?? throw new NotAuthorizedException("The given token claim is not valid");
        return new AuthenticationCandinate()
        {
            IdentityId = userId,
            Issuer = tokenIssuer,
            Audience = tokenAudience,
        };
    }

    /// <summary>
    /// Verifies that the user has accepted the latest terms of service
    /// </summary>
    public async Task VerifyPersonTermsOfServiceAgreement(Guid personId, string audience)
    {
        if (!_setup.Options.TermsOfServiceAgreementRequired) return;
        // Fetch person terms of service agreement
        _ = await _termsOfServiceRepository.GetTermsOfServiceAgreementOfTheLatestTermsByPersonId(personId, audience) ?? throw new NotAuthorizedException("User has not accepted the latest terms of service.");
    }
}