using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
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

    [HttpGet("identity/testbed/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        var user = await _mediator.Send(new GetTestbedIdentityUser.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer));

        if (user is null)
            return NotFound();

        return Ok(user);
    }
}