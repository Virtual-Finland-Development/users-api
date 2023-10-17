using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Services;

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

    protected async Task<Guid?> GetCurrentUserId(bool verifyTermsOfServiceAgreement = true)
    {
        return await _authenticationService.GetCurrentUserId(this.Request, verifyTermsOfServiceAgreement);
    }

    /// <summary>
    /// If user is not found in database, create new user and return users Id
    ///  - authentication header / token should be verified before calling this method
    /// </summary>
    protected async Task<Guid> GetPersonIdOrCreateNewPersonWithId()
    {
        Guid personId;
        try
        {
            personId = await _authenticationService.GetCurrentUserId(Request);
        }
        catch (Exception e)
        {
            if (e is not NotFoundException && e is not NotAuthorizedException)
            {
                throw;
            }

            var jwkToken = await _authenticationService.ParseAuthenticationHeader(Request);
            var query = new VerifyIdentityUser.Query(jwkToken.UserId, jwkToken.Issuer);
            var createdUser = await _mediator.Send(query);
            personId = createdUser.Id;
        }

        return personId;
    }
}