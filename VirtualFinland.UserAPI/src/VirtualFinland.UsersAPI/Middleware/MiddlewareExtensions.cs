namespace VirtualFinland.UserAPI.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseIdentityProviderAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdentityProviderAuthMiddleware>();
    }
}