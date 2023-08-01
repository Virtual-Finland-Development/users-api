using System.IdentityModel.Tokens.Jwt;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class UserSecurityService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<UserSecurityService> _logger;

    public UserSecurityService(IPersonsRepository personsRepository, ILogger<UserSecurityService> logger)
    {
        _personsRepository = personsRepository;
        _logger = logger;
    }

    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    public async Task<Person> VerifyAndGetAuthenticatedUser(string token)
    {
        var jwtTokenResult = ParseJWTToken(token);

        try
        {
            return await _personsRepository.GetPerson(jwtTokenResult.Issuer, jwtTokenResult.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", jwtTokenResult.UserId, jwtTokenResult.Issuer);
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }

    /// <summary>
    /// Parses the JWT token and returns the issuer and the user id
    /// </summary>
    public JWTTokenResult ParseJWTToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new NotAuthorizedException("No token provided");
        }

        var issuer = GetTokenIssuer(token);
        var userId = GetTokenUserId(token);

        if (userId == null || issuer == null)
        {
            throw new NotAuthorizedException("The given token is not valid");
        }
        return new JWTTokenResult() { UserId = userId, Issuer = issuer };
    }

    private static string? GetTokenUserId(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return string.Empty;
        }

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
        return string.IsNullOrEmpty(jwtSecurityToken.Subject) ? jwtSecurityToken.Claims.FirstOrDefault(o => o.Type == "userId")?.Value : jwtSecurityToken.Subject;
    }

    private static string? GetTokenIssuer(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var canReadToken = tokenHandler.CanReadToken(token);
        return canReadToken ? tokenHandler.ReadJwtToken(token).Issuer : string.Empty;
    }

    public class JWTTokenResult
    {
        public string UserId { get; set; } = null!;
        public string Issuer { get; set; } = null!;
    }
}
