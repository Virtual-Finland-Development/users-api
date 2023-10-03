
using MediatR;
using VirtualFinland.UserAPI.Helpers.Swagger;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers;

public abstract class AuthenticatedRequest<T> : IRequest<T>
{
    [SwaggerIgnore]
    public RequestAuthenticatedUser RequestAuthenticatedUser { get; set; } = default!;

    public AuthenticatedRequest()
    {
    }

    public AuthenticatedRequest(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        RequestAuthenticatedUser = RequestAuthenticatedUser;
    }

    public void SetAuth(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        RequestAuthenticatedUser = RequestAuthenticatedUser;
    }
}

public abstract class AuthenticatedRequest : IRequest
{
    [SwaggerIgnore]
    public RequestAuthenticatedUser RequestAuthenticatedUser { get; set; } = default!;

    public AuthenticatedRequest()
    {
    }

    public AuthenticatedRequest(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        RequestAuthenticatedUser = RequestAuthenticatedUser;
    }

    public void SetAuth(RequestAuthenticatedUser RequestAuthenticatedUser)
    {
        RequestAuthenticatedUser = RequestAuthenticatedUser;
    }
}