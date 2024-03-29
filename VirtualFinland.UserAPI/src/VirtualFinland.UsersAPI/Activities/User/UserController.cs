using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.User.Occupations.Operations;
using VirtualFinland.UserAPI.Activities.User.Operations;
using VirtualFinland.UserAPI.Activities.User.Operations.TermsOfServiceAgreement;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Activities.User;

[ApiController]
[Authorize]
[Authorize(Policy = "RequestFromAccessFinland")]
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
        return Ok(await _mediator.Send(new GetUser.Query(await Authenticate())));
    }

    [HttpPatch("/user")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile", Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        command.SetAuth(await Authenticate());
        return Ok(await _mediator.Send(command));
    }

    [HttpDelete("/user")]
    [SwaggerOperation(Summary = "Deletes the current logged user personal profile", Description = "Deletes the current logged user own personal details job applicant profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteUser()
    {
        var command = new DeleteUser.Command();

        command.SetAuth(await Authenticate(false));
        return Ok(await _mediator.Send(command));
    }

    [HttpGet("/user/search-profiles/")]
    [ProducesResponseType(typeof(IList<GetSearchProfiles.SearchProfile>), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IList<GetSearchProfiles.SearchProfile>> GetUserSearchProfiles()
    {
        return await _mediator.Send(new GetSearchProfiles.Query(await Authenticate()));
    }

    [HttpGet("/user/search-profiles/{profileId}")]
    [ProducesResponseType(typeof(GetSearchProfile.SearchProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetUserSearchProfile(Guid profileId)
    {
        var searchProfile = await _mediator.Send(new GetSearchProfile.Query(await Authenticate(), profileId));

        return Ok(searchProfile);
    }

    [HttpPatch("/user/search-profiles/{profileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUserSearchProfile(UpdateSearchProfile.Command command)
    {
        command.SetAuth(await Authenticate());
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("/user/search-profiles")]
    [ProducesResponseType(typeof(CreateSearchProfile.SearchProfile), StatusCodes.Status201Created)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> CreateUserSearchProfile(CreateSearchProfile.Command command)
    {
        command.SetAuth(await Authenticate());
        var searchProfile = await _mediator.Send(command);

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
        command.SetAuth(await Authenticate());

        var result = await _mediator.Send(command);

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
        command.SetAuth(await Authenticate());

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("/user/occupations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteSelectedOccupations(List<Guid> ids)
    {
        var command = new DeleteOccupations.Command(ids);
        command.SetAuth(await Authenticate(false));

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpPost("/user/occupations:delete-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteAllOccupations()
    {
        var command = new DeleteOccupations.Command();
        command.SetAuth(await Authenticate(false));

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpGet("/user/terms-of-service-agreement")]
    [SwaggerOperation(Summary = "Get the user terms agreement status",
        Description = "Returns the current logged user terms agreement status.")]
    [ProducesResponseType(typeof(GetUser.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonTermsOfServiceAgreement()
    {
        var authenticated = await AuthenticateOrRegisterPerson(false);
        return Ok(await _mediator.Send(new GetPersonServiceTermsAgreement.Query(authenticated)));
    }

    [HttpPost("/user/terms-of-service-agreement")]
    [SwaggerOperation(Summary = "Update the current logged user terms agreement status",
        Description = "Updates the current logged user terms agreement status.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdatePersonTermsOfServiceAgreement(UpdatePersonServiceTermsAgreement.Command command)
    {
        var authenticated = await Authenticate(false) ?? throw new NotAuthorizedException("User not found");
        command.SetAuth(authenticated);
        return Ok(await _mediator.Send(command));
    }

}
