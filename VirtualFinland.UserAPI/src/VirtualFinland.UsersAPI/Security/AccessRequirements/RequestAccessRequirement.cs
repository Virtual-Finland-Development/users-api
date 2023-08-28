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
        if (!_config.IsEnabled)
            context.Succeed(requirement);

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
        foreach (var accessKey in _config.AccessKeys)
        {
            if (accessKey == "") continue;
            if (apiKey == accessKey)
                return true;
        }
        return false;
    }
}