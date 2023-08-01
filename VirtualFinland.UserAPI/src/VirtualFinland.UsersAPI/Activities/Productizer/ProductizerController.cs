using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation;
using VirtualFinland.UserAPI.Activities.Productizer.Operations.JobApplicantProfile;
using VirtualFinland.UserAPI.Data.Repositories;
using VirtualFinland.UserAPI.Exceptions;
using VirtualFinland.UserAPI.Helpers.Services;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Activities.Productizer;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class ProductizerController : ControllerBase
{
    private readonly AuthenticationService _authenticationService;
    private readonly TestbedConsentSecurityService _consentSecurityService;
    private readonly ILogger<ProductizerController> _logger;

    private readonly IMediator _mediator;
    private readonly string _userProfileDataSourceURI;
    private readonly IPersonsRepository _personsRepository;

    public ProductizerController(
        IMediator mediator,
        AuthenticationService authenticationService,
        TestbedConsentSecurityService consentSecurityService,
        ILogger<ProductizerController> logger,
        IConfiguration configuration,
        IPersonsRepository personsRepository)
    {
        _mediator = mediator;
        _authenticationService = authenticationService;
        _consentSecurityService = consentSecurityService;
        _logger = logger;
        _userProfileDataSourceURI = configuration["ConsentDataSources:UserProfile"] ?? throw new ArgumentNullException("ConsentDataSources:UserProfile");
        _personsRepository = personsRepository;
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
        var user = await _authenticationService.GetCurrentUser(Request);
        return Ok(await _mediator.Send(new GetUser.Query(user?.Id, user?.DataAccessKey)));
    }

    [HttpPost("/productizer/test/lassipatanen/User/Profile/Write")]
    [SwaggerOperation(Summary = "Updates the current logged user personal profile (Testbed Productizer)",
        Description = "Updates the current logged user own personal details and his default search profile.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateUser(UpdateUser.Command command)
    {
        await _consentSecurityService.VerifyConsentTokenRequestHeaders(Request.Headers, _userProfileDataSourceURI);
        var user = await _authenticationService.GetCurrentUser(Request);
        command.SetAuth(user?.Id, user?.DataAccessKey);
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/BasicInformation")]
    [SwaggerOperation(Summary = "Get person basic information",
        Description = "Gets data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonBasicInformation()
    {
        Person? user;
        try
        {
            user = await _authenticationService.GetCurrentUser(Request);
        }
        catch (NotAuthorizedException)
        {
            _logger.LogInformation(
                "Person was not found in database while trying to retrieve person basic information");
            throw new NotFoundException("Person not found");
        }

        var result = await _mediator.Send(new GetPersonBasicInformation.Query(user?.Id, user?.DataAccessKey));

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
        var user = await GetOrCreateNewPerson();
        command.SetAuth(user?.Id, user?.DataAccessKey);
        return Ok(await _mediator.Send(command));
    }

    [HttpPost("productizer/draft/Person/JobApplicantProfile")]
    [SwaggerOperation(Summary = "Get person job applicant profile",
        Description = "Gets data product matching endpoint path from Testbed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<IActionResult> GetPersonJobApplicantInformation()
    {
        Person? person;
        try
        {
            person = await _authenticationService.GetCurrentUser(Request);
        }
        catch (NotAuthorizedException)
        {
            _logger.LogInformation(
                "Person was not found in database while trying to retrieve person job applicant profile");
            throw new NotFoundException("Person not found");
        }

        var result = await _mediator.Send(new GetJobApplicantProfile.Query(person?.Id, person?.DataAccessKey));

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
        var user = await GetOrCreateNewPerson();
        command.SetAuth(user?.Id, user?.DataAccessKey);
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    ///     If user is not found in database, create new user and return users Id
    ///     - authentication header / token should be verified before calling this method
    /// </summary>
    private async Task<Person?> GetOrCreateNewPerson()
    {
        Person? person;
        try
        {
            person = await _authenticationService.GetCurrentUser(Request);
        }
        catch (NotAuthorizedException e)
        {
            _logger.LogInformation("Could not get userId for user with error message: {Error}. Try create new user",
                e.Message);
            try
            {
                var jwkToken = _authenticationService.ParseAuthenticationHeader(Request);
                person = await _personsRepository.GetOrCreatePerson(jwkToken.Issuer, jwkToken.UserId, CancellationToken.None);
                _logger.LogInformation("New user was created with Id: {UserId}", person.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError("Could not create new user. Error message: {Error}", exception.Message);
                throw;
            }
        }

        return person;
    }
}
