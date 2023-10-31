using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Middleware;

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
        if (context.Request.Headers.TryGetValue(Constants.Headers.XRequestTraceId, out var traceId))
        {
            context.TraceIdentifier = traceId;
        }

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
