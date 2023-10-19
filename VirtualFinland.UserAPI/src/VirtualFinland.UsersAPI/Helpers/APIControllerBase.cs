using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers;

public class ApiControllerBase : ControllerBase
{
    protected readonly IMediator _mediator;
    protected readonly AuthenticationService _authenticationService;

    public ApiControllerBase(IMediator mediator, AuthenticationService authenticationService)
    {
        _mediator = mediator;
        _authenticationService = authenticationService;
    }

    protected async Task<RequestAuthenticatedUser> Authenticate(bool verifyTermsOfServiceAgreement = true)
    {
        return await _authenticationService.Authenticate(HttpContext, verifyTermsOfServiceAgreement);
    }

    protected async Task<RequestAuthenticatedUser> AuthenticateOrRegisterPerson()
    {
        try
        {
            return await _authenticationService.Authenticate(HttpContext);
        }
        catch (NotFoundException)
        {
            var query = new VerifyIdentityPerson.Query(HttpContext);
            _ = await _mediator.Send(query);
            return HttpContext.Items["User"] as RequestAuthenticatedUser ?? throw new Exception("Unknown error occurred on registering");
        }
    }
}