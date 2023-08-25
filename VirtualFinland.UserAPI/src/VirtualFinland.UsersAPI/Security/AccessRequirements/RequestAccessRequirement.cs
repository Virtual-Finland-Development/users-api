using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace VirtualFinland.UserAPI.Security.AccessRequirements;

public class RequestAccessRequirement : AuthorizationHandler<RequestAccessRequirement>, IAuthorizationRequirement
{
    private readonly string _headerName;
    private readonly string _accessKey;
    public RequestAccessRequirement(RequestAccessConfig config)
    {
        _headerName = config.HeaderName;
        _accessKey = config.AccessKey;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequestAccessRequirement requirement)
    {
        if (context.Resource is DefaultHttpContext httpContext &&
            httpContext.Request.Headers.TryGetValue(_headerName, out var apiKeyHeader))
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
        return apiKey == _accessKey;
    }
}