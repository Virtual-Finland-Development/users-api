using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Middleware;

/// <summary>
/// Adds analytics to the request pipeline (for authenticated user requests)
/// </summary>
public class AnalyticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AnalyticsService _analyticsService;

    public AnalyticsMiddleware(RequestDelegate next, AnalyticsService analyticsService)
    {
        _next = next;
        _analyticsService = analyticsService;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next.Invoke(context);

        var authenticatedUser = context.Items["User"];
        if (authenticatedUser is RequestAuthenticatedUser user)
        {
            var requestPath = context.Request.Path.Value;
            var requestMethod = context.Request.Method;
            var requestContext = $"{requestMethod} {requestPath}";

            await _analyticsService.HandleAuthenticatedRequest(user, requestContext);
        }
    }
}
