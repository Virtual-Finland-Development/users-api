using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Identity.Operations;

namespace VirtualFinland.UserAPI.Activities.Identity;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;

    public IdentityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("identity/verify")]
    [SwaggerOperation(Summary = "Verifies the existence of a user that was identified by an external identity provider.",
        Description =
            "Given the access token from an external identity provider, the operation tries to find if the user exists in the system database and creates the user into the system. Notice: The user can't access the API other paths without being created into the system with this call.")]
    [ProducesResponseType(typeof(VerifyIdentityPerson.User), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> VerifyIdentityPerson()
    {
        var user = await _mediator.Send(
            new VerifyIdentityPerson.Query(
                HttpContext
            )
        );

        return Ok(user);
    }
}
