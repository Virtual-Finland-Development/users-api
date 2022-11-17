using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Activities.Productizer;

[ApiController]
// [Authorize("ProductizerAuth")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class ProductizerController : ApiControllerBase
{
    public ProductizerController(IMediator mediator, AuthenticationService authenticationService) : base(mediator, authenticationService)
    {
        
    }
    
    [HttpPost("/productizer/user")]
    [SwaggerOperation(Summary = "Get the current logged user personal profile (Testbed Productizer)", Description = "Returns the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(typeof(GetUser.User),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        return Ok(await Mediator.Send(new GetUser.Query(await this.GetCurrentUserId(), this.Request.Headers.Authorization)));
    }
    
    [HttpPatch("/productizer/user/write")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile (Testbed Productizer)", Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        command.SetAuth(await this.GetCurrentUserId());
        command.SetRequestAuthorization(this.Request.Headers.Authorization);
        return Ok(await Mediator.Send(command));
    }

}