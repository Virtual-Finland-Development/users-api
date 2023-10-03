
using MediatR;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers;

public abstract class AuthenticatedRequest<T> : IRequest<T>
{
    [SwaggerIgnore]
    public AuthenticatedUser AuthenticatedUser { get; set; } = default!;

    public AuthenticatedRequest()
    {
    }

    public AuthenticatedRequest(AuthenticatedUser authenticatedUser)
    {
        AuthenticatedUser = authenticatedUser;
    }

    public void SetAuth(AuthenticatedUser authenticatedUser)
    {
        AuthenticatedUser = authenticatedUser;
    }
}

public abstract class AuthenticatedRequest : IRequest
{
    [SwaggerIgnore]
    public AuthenticatedUser AuthenticatedUser { get; set; } = default!;

    public AuthenticatedRequest()
    {
    }

    public AuthenticatedRequest(AuthenticatedUser authenticatedUser)
    {
        AuthenticatedUser = authenticatedUser;
    }

    public void SetAuth(AuthenticatedUser authenticatedUser)
    {
        AuthenticatedUser = authenticatedUser;
    }
}