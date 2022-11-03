using System.Security.Claims;
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
    [SwaggerOperation(Summary = "Verifies the existence of a Testbed identified user.",
        Description =
            "Given the access token from Testbed, the operation tries to find if the user exists in the system database and if the user does not exist create an account. Notice: The user can't access personal information without being created into the system with this call.")]
    [ProducesResponseType(typeof(VerifyIdentityUser.User), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> VerifyIdentityUser()
    {
        var user = await _mediator.Send(new VerifyIdentityUser.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? this.User.FindFirst("nameID")?.Value, this.User.Claims.First().Issuer));

        return Ok(user);
    }
}