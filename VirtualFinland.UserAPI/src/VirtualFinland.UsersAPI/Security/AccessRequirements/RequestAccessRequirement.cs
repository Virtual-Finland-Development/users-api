using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace VirtualFinland.UserAPI.Security.AccessRequirements;

public class RequestAccessRequirement : AuthorizationHandler<RequestAccessRequirement>, IAuthorizationRequirement
{
    private readonly RequestAccessConfig _config;
    public RequestAccessRequirement(RequestAccessConfig config)
    {
        _config = config;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequestAccessRequirement requirement)
    {
        if (context.Resource is DefaultHttpContext httpContext &&
            httpContext.Request.Headers.TryGetValue(_config.HeaderName, out var apiKeyHeader))
        {
            string apiKey = apiKeyHeader.ToString();
            if (IsValidApiKey(apiKey))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }

    private bool IsValidApiKey(string apiKey)
    {
        foreach (var _accessKey in _config.AccessKeys)
            if (apiKey == _accessKey)
                return true;
        return false;
    }
}