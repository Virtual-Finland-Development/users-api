using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Activities.Productizer;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class ProductizerController : ControllerBase
{
    private readonly AuthenticationService _authenticationService;
    private readonly AuthGwVerificationService _authGwVerificationService;
    private readonly IMediator _mediator;

    public ProductizerController(
        IMediator mediator,
        AuthGwVerificationService authGwVerificationService, AuthenticationService authenticationService)
    {
        _mediator = mediator;
        _authGwVerificationService = authGwVerificationService;
        _authenticationService = authenticationService;
    }

    


    [HttpPost("/productizer/test/lassipatanen/User/Profile")]
    [SwaggerOperation(Summary = "Get the current logged user personal profile (Testbed Productizer)",
        Description = "Returns the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(typeof(GetUser.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        await _authGwVerificationService.VerifyTokens(Request, true);
        return Ok(await _mediator.Send(new GetUser.Query(await _authenticationService.GetCurrentUserId(Request))));
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile/Write")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile (Testbed Productizer)",
        Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        await _authGwVerificationService.VerifyTokens(Request, true);
        command.SetAuth(await _authenticationService.GetCurrentUserId(Request));
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/BasicInformation")]
    [SwaggerOperation(Summary = "Get person basic information",
        Description = "Gets dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonBasicInformation()
    {
        await _authGwVerificationService.VerifyTokens(Request, true);
        return Ok(await _mediator.Send(new GetPersonBasicInformation.Query(await GetUserIdOrCreateNewUserWithId())));
    }

    [HttpPost("productizer/draft/Person/BasicInformation/Write")]
    [SwaggerOperation(Summary = "Update person basic information",
        Description = "Updates dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonBasicInformation(
        UpdatePersonBasicInformation.Command command)
    {
        await _authGwVerificationService.VerifyTokens(Request, true);
        command.SetAuth(await GetUserIdOrCreateNewUserWithId());
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/JobApplicantProfile")]
    [SwaggerOperation(Summary = "Get person job applicant profile",
        Description = "Gets dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonJobApplicantInformation()
    {
        await _authGwVerificationService.VerifyTokens(Request, true);
        return Ok(await _mediator.Send(new GetJobApplicantProfile.Query(await GetUserIdOrCreateNewUserWithId())));
    }
    
    [HttpPost("productizer/draft/Person/JobApplicantProfile/Write")]
    [SwaggerOperation(Summary = "Update person job applicant profile",
        Description = "Updates dataproduct matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonJobApplicantProfile(UpdateJobApplicantProfile.Command command)
    {
        await _authGwVerificationService.VerifyTokens(Request, true);
        command.SetAuth(await GetUserIdOrCreateNewUserWithId());
        return Ok(await _mediator.Send(command));
    }

    private async Task<Guid?> GetUserIdOrCreateNewUserWithId()
    {
        Guid? userId;
        try
        {
            userId = await _authenticationService.GetCurrentUserId(Request);
        }
        catch (NotAuthorizedException e)
        {
            Console.WriteLine(e.Message);
            try
            {
                var claimsUserId =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    User.FindFirst(Constants.Web.ClaimUserId)?.Value;
                var claimsIssuer = User.Claims.First().Issuer;
                var query = new VerifyIdentityUser.Query(claimsUserId, claimsIssuer);
                var createdUser = await _mediator.Send(query);
                userId = createdUser.Id;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }

        return userId;
    }
}
