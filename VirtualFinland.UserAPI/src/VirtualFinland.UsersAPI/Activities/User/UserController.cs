using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Middleware;

namespace VirtualFinland.UserAPI.Activities.User;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class UserController : ApiControllerBase
{
    public UserController(IMediator mediator, AuthenticationService authenticationService) : base(mediator, authenticationService)
    {
        
    }
    

    [HttpGet("/user")]
    [SwaggerOperation(Summary = "Get the current logged user personal profile", Description = "Returns the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(typeof(GetUser.User),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        return Ok(await Mediator.Send(new GetUser.Query(await this.GetCurrentUserId())));
    }
    
    [HttpPatch("/user")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile", Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        command.SetAuth(await this.GetCurrentUserId());
        return Ok(await Mediator.Send(command));
    }
    
    [HttpGet("/user/consents")]
    [SwaggerOperation(Summary = "Get the current logged user personal consents", Description = "Returns the current logged user own personal consents.")]
    [ProducesResponseType(typeof(GetConsents.Consents),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetUserConsents()
    {
        return Ok(await Mediator.Send(new GetConsents.Query(await this.GetCurrentUserId())));
    }
    
    [HttpPatch("/user/consents")]
    [SwaggerOperation(Summary = "Updates the current logged user personal consents", Description = "Updates the current logged user own personal consents.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUserConsents(UpdateConsents.Command command)
    {
        command.SetAuth(await this.GetCurrentUserId());
        return Ok(await Mediator.Send(command));
    }
    
    [HttpGet("/user/search-profiles/")]
    [ProducesResponseType(typeof(IList<GetSearchProfiles.SearchProfile>),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IList<GetSearchProfiles.SearchProfile>> GetUserSearchProfiles()
    {
        return await Mediator.Send(new GetSearchProfiles.Query(await this.GetCurrentUserId()));
    }
    
    [HttpGet("/user/search-profiles/{profileId}")]
    [ProducesResponseType( typeof(GetSearchProfile.SearchProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetUserSearchProfile(Guid profileId)
    {
        var searchProfile = await Mediator.Send(new GetSearchProfile.Query(await this.GetCurrentUserId(), profileId));

        return Ok(searchProfile);
    }
    
    [HttpPatch("/user/search-profiles/{profileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUserSearchProfile(UpdateSearchProfile.Command command, Guid profileId)
    {
        command.SetAuth(await this.GetCurrentUserId());
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpPost("/user/search-profiles")]
    [ProducesResponseType(typeof(CreateSearchProfile.SearchProfile), StatusCodes.Status201Created)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> CreateUserSearchProfile(CreateSearchProfile.Command command)
    {
        command.SetAuth(await this.GetCurrentUserId());
        var searchProfile = await Mediator.Send(command);

        return CreatedAtAction(nameof(GetUserSearchProfile), new
        {
            profileId = searchProfile.Id
        }, searchProfile);
    }

}