using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Activities.Productizer;

[ApiController]
[Authorize]
[Authorize(Policy = "RequestFromDataspace")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class ProductizerController : ApiControllerBase
{
    private readonly TestbedConsentSecurityService _consentSecurityService;
    private readonly ILogger<ProductizerController> _logger;

    private readonly string _userProfileDataSourceURI;

    public ProductizerController(
        IMediator mediator,
        AuthenticationService authenticationService,
        TestbedConsentSecurityService consentSecurityService,
        ILogger<ProductizerController> logger,
        IConfiguration configuration) : base(mediator, authenticationService)
    {
        _consentSecurityService = consentSecurityService;
        _logger = logger;
        _userProfileDataSourceURI = configuration["ConsentDataSources:UserProfile"] ?? throw new ArgumentNullException("ConsentDataSources:UserProfile");
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile")]
    [SwaggerOperation(Summary = "Get the current logged user personal profile (Testbed Productizer)",
        Description = "Returns the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(typeof(GetUser.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        await _consentSecurityService.VerifyConsentTokenRequestHeaders(Request.Headers, _userProfileDataSourceURI);
        return Ok(await _mediator.Send(new GetUser.Query(await _authenticationService.GetCurrentUserId(Request))));
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile/Write")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile (Testbed Productizer)",
        Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        await _consentSecurityService.VerifyConsentTokenRequestHeaders(Request.Headers, _userProfileDataSourceURI);
        command.SetAuth(await _authenticationService.GetCurrentUserId(Request));
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/Person/BasicInformation_v0.1")]
    [HttpPost("productizer/Person/BasicInformation_v1.0")]
    [SwaggerOperation(Summary = "Get person basic information",
        Description = "Gets data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonBasicInformation()
    {
        var personId = await _authenticationService.GetCurrentUserId(Request);
        var result = await _mediator.Send(new GetPersonBasicInformation.Query(personId));
        if (!ProductizerProfileValidator.IsPersonBasicInformationCreated(result)) throw new NotFoundException("Person not found");

        return Ok(result);
    }

    [HttpPost("productizer/Person/BasicInformation/Write_v0.1")]
    [HttpPost("productizer/Person/BasicInformation/Write_v1.0")]
    [SwaggerOperation(Summary = "Update person basic information",
        Description = "Updates data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonBasicInformation(
        UpdatePersonBasicInformation.Command command)
    {
        command.SetAuth(await GetPersonIdOrCreateNewPersonWithId());
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/Person/JobApplicantProfile_v0.1")]
    [HttpPost("productizer/Person/JobApplicantProfile_v1.0")]
    [SwaggerOperation(Summary = "Get person job applicant profile",
        Description = "Gets data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonJobApplicantInformation()
    {
        var personId = await _authenticationService.GetCurrentUserId(Request);
        var result = await _mediator.Send(new GetJobApplicantProfile.Query(personId));
        if (!ProductizerProfileValidator.IsJobApplicantProfileCreated(result)) throw new NotFoundException("Job applicant profile not found");

        return Ok(result);
    }

    [HttpPost("productizer/Person/JobApplicantProfile/Write_v0.1")]
    [HttpPost("productizer/Person/JobApplicantProfile/Write_v1.0")]
    [SwaggerOperation(Summary = "Update person job applicant profile",
        Description = "Updates data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> SaveOrUpdatePersonJobApplicantProfile(UpdateJobApplicantProfile.Command command)
    {
        command.SetAuth(await GetPersonIdOrCreateNewPersonWithId());
        return Ok(await _mediator.Send(command));
    }
}
