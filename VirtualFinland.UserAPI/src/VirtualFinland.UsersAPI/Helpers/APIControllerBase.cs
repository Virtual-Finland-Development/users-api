using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Middleware;

namespace VirtualFinland.UserAPI.Helpers;

public class ApiControllerBase : ControllerBase
{
    private readonly UsersDbContext _usersDbContext;
    private readonly ILogger<ApiControllerBase> _logger;
    private readonly IConfiguration _configuration;
    protected readonly IMediator Mediator;
    private Guid? _currentUserId;
    public ApiControllerBase( UsersDbContext usersDbContext, ILogger<ApiControllerBase> logger, IMediator mediator, IConfiguration configuration)
    {
        _usersDbContext = usersDbContext;
        _logger = logger;
        _configuration = configuration;
        Mediator = mediator;
    }

    protected async Task<Guid?> GetCurrentUserId()
    {
            if (this._currentUserId == null)
            {
                if (this.Request.Path.Value != null && this.User != null && this.User.Identity != null && this.User.Identity.IsAuthenticated && !this.Request.Path.Value.Contains("/identity"))
                {
                    var dbUser = await VerifyAndGetAuthenticatedUser(this.User.Claims.First().Issuer, this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? this.User.FindFirst(Constants.Web.ClaimNameId)?.Value);
                    _currentUserId = dbUser?.Id;
                }
            }
            return (Guid?)this._currentUserId;
    }

    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="claimsIssuer"></param>
    /// <param name="claimsUserId"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    private async Task<Models.UsersDatabase.User?> VerifyAndGetAuthenticatedUser(string? claimsIssuer, string? claimsUserId)
    {
        try
        {
            if (claimsIssuer.Contains(_configuration["SuomiFI:Issuer"]))
            {
                claimsUserId = "suomifiDummyUserId";
            }
            
            var externalIdentity = await _usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == claimsUserId && o.Issuer == claimsIssuer, CancellationToken.None);
            return await _usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", claimsUserId, claimsIssuer);
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }
}