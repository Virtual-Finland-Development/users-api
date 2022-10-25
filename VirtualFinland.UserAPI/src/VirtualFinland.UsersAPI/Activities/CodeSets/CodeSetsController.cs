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

    [HttpGet("/code-sets/countries")]
    [SwaggerOperation(Summary = "Get all available ISO 3166 country codes and details", Description = "Get all available ISO 3166 country codes and details.")]
    [ProducesResponseType(typeof(List<GetAllCountries.Country>),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetAllCountries()
    {
        return Ok(await _mediator.Send(new GetAllCountries.Query()));
    }
    
    [HttpGet("/code-sets/countries/{countryCode}")]
    [SwaggerOperation(Summary = "Gets a single country and its details by it's Two Letter ISO Code", Description = "Gets a single country and its details by it's Two Letter ISO Code.")]
    [ProducesResponseType(typeof(GetCountry.Country),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetCountry(string countryCode)
    {
        return Ok(await _mediator.Send(new GetCountry.Query(countryCode)));
    }
    
    [HttpGet("/code-sets/occupations")]
    [SwaggerOperation(Summary = "Get all available ISCO occupations", Description = "Get all available ISCO occupations")]
    [ProducesResponseType(typeof(List<GetAllOccupations.Occupation>),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetAllOccupations()
    {
        return Ok(await _mediator.Send(new GetAllOccupations.Query()));
    }
    
    [HttpGet("/code-sets/occupations/{occupationCode}")]
    [SwaggerOperation(Summary = "Gets a single occupation and its details by it's ISCO code", Description = "Gets a single occupation and its details by it's ISCO code.")]
    [ProducesResponseType(typeof(GetOccupation.Occupation),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetOccupation(string occupationCode)
    {
        return Ok(await _mediator.Send(new GetOccupation.Query(occupationCode)));
    }
    
    [HttpGet("/code-sets/languages")]
    [SwaggerOperation(Summary = "Get all languages", Description = "Get all languages.")]
    [ProducesResponseType(typeof(List<GetAllLanguages.Language>),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetAllLanguages()
    {
        return Ok(await _mediator.Send(new GetAllLanguages.Query()));
    }
    
    [HttpGet("/code-sets/genders")]
    [SwaggerOperation(Summary = "Get all gender options", Description = "Get all gender options.")]
    [ProducesResponseType(typeof(List<GetAllGenders.Gender>),StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetAllGenders()
    {
        return Ok(await _mediator.Send(new GetAllGenders.Query()));
    }
}