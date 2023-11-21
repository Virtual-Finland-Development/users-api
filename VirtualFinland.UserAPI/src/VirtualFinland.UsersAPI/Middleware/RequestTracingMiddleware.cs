using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.UserAPI.Middleware;

/// <summary>
/// Overrides the trace identifier with the value of the X-Request-TraceId header (if present)
/// </summary>
public class RequestTracingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTracingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(Constants.Headers.XRequestTraceId, out var traceId))
        {
            context.TraceIdentifier = traceId;
        }

        await _next.Invoke(context);
    }
}