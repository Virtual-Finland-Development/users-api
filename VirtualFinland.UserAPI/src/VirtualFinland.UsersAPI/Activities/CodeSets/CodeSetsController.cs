using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.CodeSets.Operations;
using VirtualFinland.UserAPI.Activities.User.Operations;

namespace VirtualFinland.UserAPI.Activities.CodeSets;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
public class CodeSetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CodeSetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/code-sets/countries")]
    [SwaggerOperation(Summary = "Get all available ISO 3166 country codes and details", Description = "Get all available ISO 3166 country codes and details.")]
    [ProducesResponseType(typeof(List<GetAllCountries.Country>),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    public async Task<IActionResult> GetTestbedIdentityUser()
    {
        return Ok(await _mediator.Send(new GetAllCountries.Query()));
    }
}