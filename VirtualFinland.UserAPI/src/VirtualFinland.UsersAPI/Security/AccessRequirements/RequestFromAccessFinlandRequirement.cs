using Microsoft.AspNetCore.Authorization;
using VirtualFinland.UserAPI.Security.Configurations;

namespace VirtualFinland.UserAPI.Security.AccessRequirements;

public class RequestFromAccessFinlandRequirement : AuthorizationHandler<RequestFromAccessFinlandRequirement>, IAuthorizationRequirement
{
    private readonly string _accessKey;
    public RequestFromAccessFinlandRequirement(IConfiguration configuration)
    {
        _accessKey = configuration["Security:Access:SharedAccessKey"];
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequestFromAccessFinlandRequirement requirement)
    {
        if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcContext &&
            mvcContext.HttpContext.Request.Headers.TryGetValue("x-api-key", out var apiKeyHeader))
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