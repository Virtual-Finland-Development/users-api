using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.User.Occupations.Operations;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;

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
    [ProducesResponseType(typeof(GetUser.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        var user = await this.GetCurrentUser();
        return Ok(await Mediator.Send(new GetUser.Query(user?.Id, user?.DataAccessKey)));
    }

    [HttpPatch("/user")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile", Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id, user?.DataAccessKey);
        return Ok(await Mediator.Send(command));
    }

    [HttpDelete("/user")]
    [SwaggerOperation(Summary = "Deletes the current logged user personal profile", Description = "Deletes the current logged user own personal details job applicant profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteUser()
    {
        var command = new DeleteUser.Command();
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id);
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("/user/search-profiles/")]
    [ProducesResponseType(typeof(IList<GetSearchProfiles.SearchProfile>), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IList<GetSearchProfiles.SearchProfile>> GetUserSearchProfiles()
    {
        var user = await this.GetCurrentUser();
        return await Mediator.Send(new GetSearchProfiles.Query(user?.Id));
    }

    [HttpGet("/user/search-profiles/{profileId}")]
    [ProducesResponseType(typeof(GetSearchProfile.SearchProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetUserSearchProfile(Guid profileId)
    {
        var user = await this.GetCurrentUser();
        var searchProfile = await Mediator.Send(new GetSearchProfile.Query(user?.Id, profileId));

        return Ok(searchProfile);
    }

    [HttpPatch("/user/search-profiles/{profileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUserSearchProfile(UpdateSearchProfile.Command command, Guid profileId)
    {
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id);
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("/user/search-profiles")]
    [ProducesResponseType(typeof(CreateSearchProfile.SearchProfile), StatusCodes.Status201Created)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> CreateUserSearchProfile(CreateSearchProfile.Command command)
    {
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id);
        var searchProfile = await Mediator.Send(command);

        return CreatedAtAction(nameof(GetUserSearchProfile), new
        {
            profileId = searchProfile.Id
        }, searchProfile);
    }

    [HttpPost("/user/occupations")]
    [ProducesResponseType(typeof(UpdateOccupations.Occupation), StatusCodes.Status201Created)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> AddOccupation(List<AddOccupations.AddOccupationsRequest> occupations)
    {
        var command = new AddOccupations.Command(occupations);
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id);

        var result = await Mediator.Send(command);

        return CreatedAtAction(
            nameof(AddOccupation),
            result
        );
    }

    [HttpPatch("/user/occupations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateOccupations(List<UpdateOccupations.Occupation> occupations)
    {
        var command = new UpdateOccupations.Command(occupations);
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id);

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("/user/occupations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteSelectedOccupations(List<Guid> ids)
    {
        var command = new DeleteOccupations.Command(ids);
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id);

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpPost("/user/occupations:delete-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteAllOccupations()
    {
        var command = new DeleteOccupations.Command();
        var user = await this.GetCurrentUser();
        command.SetAuth(user?.Id);

        await Mediator.Send(command);

        return NoContent();
    }

}
