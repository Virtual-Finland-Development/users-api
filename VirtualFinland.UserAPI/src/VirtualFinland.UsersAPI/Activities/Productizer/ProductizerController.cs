using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.User;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Security.Models;

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
    [ProducesResponseType(typeof(GetUserProfile.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        await _consentSecurityService.VerifyConsentTokenRequestHeaders(Request.Headers, _userProfileDataSourceURI);
        var RequestAuthenticatedUser = await _authenticationService.Authenticate(HttpContext);
        return Ok(await _mediator.Send(new GetUserProfile.Query(RequestAuthenticatedUser)));
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile/Write")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile (Testbed Productizer)",
        Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUserProfile.Command command)
    {
        await _consentSecurityService.VerifyConsentTokenRequestHeaders(Request.Headers, _userProfileDataSourceURI);
        var RequestAuthenticatedUser = await _authenticationService.Authenticate(HttpContext);
        command.SetAuth(RequestAuthenticatedUser);
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
        var requestAuthenticatedUser = await _authenticationService.Authenticate(HttpContext);
        var result = await _mediator.Send(new GetPersonBasicInformation.Query(requestAuthenticatedUser));
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
        var requestAuthenticatedUser = await AuthenticateOrRegisterPerson();
        command.SetAuth(requestAuthenticatedUser);
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
        var requestAuthenticatedUser = await _authenticationService.Authenticate(HttpContext);
        var result = await _mediator.Send(new GetJobApplicantProfile.Query(requestAuthenticatedUser));
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
        var RequestAuthenticatedUser = await AuthenticateOrRegisterPerson();
        command.SetAuth(RequestAuthenticatedUser);
        return Ok(await _mediator.Send(command));
    }
}
