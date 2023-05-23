using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Identity.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinlandDevelopment.Shared.Attributes;
using VirtualFinlandDevelopment.Shared.Exceptions;

namespace VirtualFinland.UserAPI.Activities.Productizer;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class ProductizerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AuthenticationService _authenticationService;
    private readonly ILogger<ProductizerController> _logger;

    public ProductizerController(IMediator mediator, AuthenticationService authenticationService,
        ILogger<ProductizerController> logger)
    {
        _mediator = mediator;
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile")]
    [SwaggerOperation(Summary = "Get the current logged user personal profile (Testbed Productizer)",
        Description = "Returns the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(typeof(GetUser.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [VerifyConsentAuthorization("test/lassipatanen/User/Profile")]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        return Ok(await _mediator.Send(new GetUser.Query(await _authenticationService.GetCurrentUserId(Request))));
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile/Write")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile (Testbed Productizer)",
        Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [VerifyConsentAuthorization("test/lassipatanen/User/Profile/Write")]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        command.SetAuth(await _authenticationService.GetCurrentUserId(Request));
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/BasicInformation")]
    [SwaggerOperation(Summary = "Get person basic information",
        Description = "Gets data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonBasicInformation()
    {
        Guid? userId;
        try
        {
            userId = await _authenticationService.GetCurrentUserId(Request);
        }
        catch (NotAuthorizedException)
        {
            _logger.LogInformation(
                "Person was not found in database while trying to retrieve person basic information");
            throw new NotFoundException("Person not found");
        }

        var result = await _mediator.Send(new GetPersonBasicInformation.Query(userId));

        if (!ProductizerProfileValidator.IsPersonBasicInformationCreated(result)) return NotFound();

        return Ok(result);
    }

    [HttpPost("productizer/draft/Person/BasicInformation/Write")]
    [SwaggerOperation(Summary = "Update person basic information",
        Description = "Updates data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonBasicInformation(
        UpdatePersonBasicInformation.Command command)
    {
        command.SetAuth(await GetUserIdOrCreateNewUserWithId());
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/JobApplicantProfile")]
    [SwaggerOperation(Summary = "Get person job applicant profile",
        Description = "Gets data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonJobApplicantInformation()
    {
        Guid? userId;
        try
        {
            userId = await _authenticationService.GetCurrentUserId(Request);
        }
        catch (NotAuthorizedException)
        {
            _logger.LogInformation(
                "Person was not found in database while trying to retrieve person job applicant profile");
            throw new NotFoundException("Person not found");
        }

        var result = await _mediator.Send(new GetJobApplicantProfile.Query(userId));

        if (!ProductizerProfileValidator.IsJobApplicantProfileCreated(result)) return NotFound();

        return Ok(result);
    }

    [HttpPost("productizer/draft/Person/JobApplicantProfile/Write")]
    [SwaggerOperation(Summary = "Update person job applicant profile",
        Description = "Updates data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonJobApplicantProfile(UpdateJobApplicantProfile.Command command)
    {
        command.SetAuth(await GetUserIdOrCreateNewUserWithId());
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    ///     If user is not found in database, create new user and return users Id
    ///     - authentication header / token should be verified before calling this method
    /// </summary>
    private async Task<Guid?> GetUserIdOrCreateNewUserWithId()
    {
        Guid? userId;
        try
        {
            userId = await _authenticationService.GetCurrentUserId(Request);
        }
        catch (NotAuthorizedException e)
        {
            _logger.LogInformation("Could not get userId for user with error message: {Error}. Try create new user",
                e.Message);
            try
            {
                var jwkToken = _authenticationService.ParseAuthenticationHeader(Request);
                var query = new VerifyIdentityUser.Query(jwkToken.UserId, jwkToken.Issuer);
                var createdUser = await _mediator.Send(query);
                userId = createdUser.Id;
                _logger.LogInformation("New user was created with Id: {UserId}", userId);
            }
            catch (Exception exception)
            {
                _logger.LogError("Could not create new user. Error message: {Error}", exception.Message);
                throw;
            }
        }

        return userId;
    }
}
