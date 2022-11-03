using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Exceptions;

namespace VirtualFinland.UserAPI.Middleware;

/// <summary>
/// This middleware makes sure that a Identity Providers token user claims correspond to a user that should exist within the users database.
/// </summary>
public class IdentityProviderAuthMiddleware
{
    private readonly RequestDelegate _next;
    public static string ContextItemUserDbIdName { get; } = "UserDBId";

    public IdentityProviderAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UsersDbContext usersDbContext, ILogger<IdentityProviderAuthMiddleware> logger)
    {
        if (context.Request.Path.Value != null && context.User.Identity != null && context.User.Identity.IsAuthenticated && !context.Request.Path.Value.Contains("/identity"))
        {
            var dbUser = await VerifyAndGetAuthenticatedUser(context.User.Claims.First().Issuer, context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("nameID")?.Value, usersDbContext, logger);
            context.Items[IdentityProviderAuthMiddleware.ContextItemUserDbIdName] = dbUser.Id;
        }

        await _next(context);
    }
    
    /// <summary>
    /// This function tries to verify that the given token has a valid created user account in the user DB. If not the client should "verify" the token through the IdentityController
    /// </summary>
    /// <param name="claimsIssuer"></param>
    /// <param name="claimsUserId"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException">If user id and the issuer are not found in the DB for any given user, this is not a valid user within the users database.</exception>
    private async Task<Models.UsersDatabase.User> VerifyAndGetAuthenticatedUser(string? claimsIssuer, string? claimsUserId, UsersDbContext usersDbContext, ILogger<IdentityProviderAuthMiddleware> logger)
    {
        try
        {
            var externalIdentity = await usersDbContext.ExternalIdentities.SingleAsync(o => o.IdentityId == claimsUserId && o.Issuer == claimsIssuer, CancellationToken.None);
            return await usersDbContext.Users.SingleAsync(o => o.Id == externalIdentity.UserId, CancellationToken.None);
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning("User could not be identified as a valid user: {RequestClaimsUserId} from issuer: {RequestClaimsIssuer}", claimsUserId, claimsIssuer);
            throw new NotAuthorizedException("User could not be identified as a valid user. Use the verify path to make sure that the given access token is valid in the system: /identity/testbed/verify", e);
        }
    }
    
}