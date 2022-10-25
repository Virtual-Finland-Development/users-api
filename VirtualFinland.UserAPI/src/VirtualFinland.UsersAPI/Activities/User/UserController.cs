using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.User.Operations;

namespace VirtualFinland.UserAPI.Activities.User;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/user")]
    [SwaggerOperation(Summary = "Get the current logged user personal profile", Description = "Returns the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(typeof(GetUser.User),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        return Ok(await _mediator.Send(new GetUser.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer)));
    }
    
    [HttpPatch("/user")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile", Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        command.SetAuth(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer);
        return Ok(await _mediator.Send(command));
    }
    
    [HttpGet("/user/consents")]
    [SwaggerOperation(Summary = "Get the current logged user personal consents", Description = "Returns the current logged user own personal consents.")]
    [ProducesResponseType(typeof(GetConsents.Consents),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetUserConsents()
    {
        return Ok(await _mediator.Send(new GetConsents.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer)));
    }
    
    [HttpPatch("/user/consents")]
    [SwaggerOperation(Summary = "Updates the current logged user personal consents", Description = "Updates the current logged user own personal consents.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUserConsents(UpdateConsents.Command command)
    {
        command.SetAuth(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer);
        return Ok(await _mediator.Send(command));
    }
    
    [HttpGet("/user/search-profiles/")]
    [ProducesResponseType(typeof(IList<GetSearchProfiles.SearchProfile>),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IList<GetSearchProfiles.SearchProfile>> GetUserSearchProfiles()
    {
        return await _mediator.Send(new GetSearchProfiles.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer));
    }
    
    [HttpGet("/user/search-profiles/{profileId}")]
    [ProducesResponseType( typeof(GetSearchProfile.SearchProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetUserSearchProfile(Guid profileId)
    {
        var searchProfile = await _mediator.Send(new GetSearchProfile.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer, profileId));

        return Ok(searchProfile);
    }
    
    [HttpPatch("/user/search-profiles/{profileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUserSearchProfile(UpdateSearchProfile.Command command, Guid profileId)
    {
        command.SetAuth(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer);
        await _mediator.Send(command);
        return NoContent();
    }
    
    [HttpPost("/user/search-profiles")]
    [ProducesResponseType(typeof(CreateSearchProfile.SearchProfile), StatusCodes.Status201Created)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> CreateUserSearchProfile(CreateSearchProfile.Command command)
    {
        command.SetAuth(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer);
        var searchProfile = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetUserSearchProfile), new
        {
            profileId = searchProfile.Id
        }, searchProfile);
    }
}