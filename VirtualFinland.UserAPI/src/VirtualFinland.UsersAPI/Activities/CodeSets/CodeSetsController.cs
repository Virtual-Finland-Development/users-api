using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VirtualFinland.UserAPI.Activities.CodeSets.Operations;

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

    [HttpGet("/code-sets/genders")]
    [SwaggerOperation(Summary = "Get all gender options", Description = "Get all gender options.")]
    [ProducesResponseType(typeof(List<GetAllGenders.Gender>), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetAllGenders()
    {
        return Ok(await _mediator.Send(new GetAllGenders.Query()));
    }
}