using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.UsersDatabase;

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

    protected async Task<Person?> GetCurrentUser()
    {
        return await _authenticationService.GetCurrentUser(this.Request);
    }
}