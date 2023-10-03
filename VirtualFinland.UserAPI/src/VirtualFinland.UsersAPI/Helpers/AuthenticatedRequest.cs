
using MediatR;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers;

public abstract class AuthenticatedRequest<T> : IRequest<T>
{
    [SwaggerIgnore]
    public RequestAuthenticatedUser User { get; set; } = default!;

    public AuthenticatedRequest()
    {
    }

    public AuthenticatedRequest(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        User = RequestAuthenticatedUser;
    }

    public void SetAuth(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        User = RequestAuthenticatedUser;
    }
}

public abstract class AuthenticatedRequest : IRequest
{
    [SwaggerIgnore]
    public RequestAuthenticatedUser User { get; set; } = default!;

    public AuthenticatedRequest()
    {
    }

    public AuthenticatedRequest(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        User = RequestAuthenticatedUser;
    }

    public void SetAuth(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        User = RequestAuthenticatedUser;
    }
}