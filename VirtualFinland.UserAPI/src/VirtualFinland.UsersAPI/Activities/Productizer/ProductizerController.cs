using System.Security;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Activities.Productizer;

[Authorize]
[ApiController]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class ProductizerController : ControllerBase
{
    private readonly AuthGwVerificationService _authGwVerificationService;
    private readonly IMediator _mediator;

    public ProductizerController(IMediator mediator, AuthGwVerificationService authGwVerificationService)
    {
        _mediator = mediator;
        _authGwVerificationService = authGwVerificationService;
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile")]
    [SwaggerOperation(Summary = "Get the current logged user personal profile (Testbed Productizer)",
        Description = "Returns the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(typeof(GetUser.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        await _authGwVerificationService.AuthGwVerification(Request, false);
        return Ok(await _mediator.Send(new GetUser.Query(await _authGwVerificationService.GetCurrentUserId(Request))));
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile/Write")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile (Testbed Productizer)",
        Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        await _authGwVerificationService.AuthGwVerification(Request, true);
        command.SetAuth(await _authGwVerificationService.GetCurrentUserId(Request));
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/BasicInformation")]
    [SwaggerOperation(Summary = "Get person basic information",
        Description = "Gets dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonBasicInformation()
    {
        await _authGwVerificationService.AuthGwVerification(Request);

        var userId = await _authGwVerificationService.GetCurrentUserId(Request);

        return Ok(await _mediator.Send(new GetPersonBasicInformation.Query(userId)));
    }

    [HttpPost("productizer/draft/Person/BasicInformation/Write")]
    [SwaggerOperation(Summary = "Get person job applicant profile",
        Description = "Gets dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonBasicInformation(
        UpdatePersonBasicInformation.Command command)
    {
        await _authGwVerificationService.AuthGwVerification(Request);
        var userId = await _authGwVerificationService.GetCurrentUserId(Request);

        // TODO: If user doesn't exist we should create new one and use the new User ID

        command.SetAuth(userId);

        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/JobApplicantProfile")]
    [SwaggerOperation(Summary = "Get person job applicant profile",
        Description = "Gets dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonJobApplicantInformation()
    {
        await _authGwVerificationService.AuthGwVerification(Request);
        var userId = await _authGwVerificationService.GetCurrentUserId(Request);

        return Ok(await _mediator.Send(new GetPersonJobApplicantProfile.Query(userId)));
    }

    [HttpPost("productizer/draft/Person/JobApplicantProfile/Write")]
    [SwaggerOperation(Summary = "Get person job applicant profile",
        Description = "Gets dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonJobApplicantProfile(
        UpdateJobApplicantProfile.Command command)
    {
        await _authGwVerificationService.AuthGwVerification(Request);

        Guid? userId;
        try
        {
            userId = await _authGwVerificationService.GetCurrentUserId(Request);
        }
        catch (VerificationException)
        {
            Console.WriteLine("User didn't exist, try create new one");
            try
            {
                var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                   User.FindFirst(Constants.Web.ClaimUserId)?.Value;
                var issuer = User.Claims.First().Issuer;

                var user = await _mediator.Send(new VerifyIdentityUser.Query(claimsUserId, issuer));

                userId = user.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        command.SetAuth(userId);

        return Ok(await _mediator.Send(command));
    }
}
