using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Helpers;

public class ApiControllerBase : ControllerBase
{
    protected readonly IMediator Mediator;
    private readonly AuthenticationService _authenticationService;
    public ApiControllerBase(IMediator mediator, AuthenticationService authenticationService)
    {
        Mediator = mediator;
        _authenticationService = authenticationService;
    }

    protected async Task<Guid?> GetCurrentUserId()
    {
        return await _authenticationService.GetCurrentUserId(this.Request);
    }
}