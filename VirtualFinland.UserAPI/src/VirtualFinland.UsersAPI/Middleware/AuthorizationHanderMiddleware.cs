using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using VirtualFinland.UserAPI.Exceptions;

public class AuthorizationHanderMiddleware : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Succeeded)
        {
            throw new NotAuthorizedException("Access denied"); // Pass to the error handler middleware
        }

        // Fall back to the default implementation.
        await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
