using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        return Ok(await _mediator.Send(new GetUser.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer)));
    }
    
    [HttpPatch("/user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(UpdateUser.UpdateUserCommand command)
    {
        command.SetAuth(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer);
        await _mediator.Send(command);
        return NoContent();
    }
    
    [HttpGet("/user/search-profiles/")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IList<GetSearchProfiles.SearchProfile>> GetUserSearchProfiles()
    {
        return await _mediator.Send(new GetSearchProfiles.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer));
    }
    
    [HttpGet("/user/search-profiles/{profileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserSearchProfile(Guid profileId)
    {
        var searchProfile = await _mediator.Send(new GetSearchProfile.Query(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer, profileId));

        if (searchProfile is null)
        {
            return NotFound();
        }

        return Ok(searchProfile);
    }
    
    [HttpPatch("/user/search-profiles/{profileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserSearchProfile(UpdateSearchProfile.UpdateSearchProfileCommand command, Guid profileId)
    {
        command.SetAuth(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer);
        await _mediator.Send(command);
        return NoContent();
    }
    
    [HttpPost("/user/search-profiles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUserSearchProfile(CreateSearchProfile.CreateSearchProfileCommand command)
    {
        command.SetAuth(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, this.User.Claims.First().Issuer);
        var searchProfile = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetUserSearchProfile), new
        {
            profileId = searchProfile.Id
        }, searchProfile);
    }
}