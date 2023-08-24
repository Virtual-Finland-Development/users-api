using Microsoft.AspNetCore.Authorization;
using VirtualFinland.UserAPI.Security.Configurations;

namespace VirtualFinland.UserAPI.Security.AccessRequirements;

public class RequestFromDataspaceRequirement : AuthorizationHandler<RequestFromDataspaceRequirement>, IAuthorizationRequirement
{
    private readonly string _accessKey;
    public RequestFromDataspaceRequirement(IConfiguration configuration)
    {
        _accessKey = configuration["Security:Access:DataspaceAgent"];
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequestFromDataspaceRequirement requirement)
    {
        // Check if the request contains the x-api-key header
        if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcContext &&
            mvcContext.HttpContext.Request.Headers.TryGetValue("user-agent", out var apiKeyHeader))
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