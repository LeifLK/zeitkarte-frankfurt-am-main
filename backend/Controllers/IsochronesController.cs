using backend.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IsochronesController : ControllerBase
{
    private readonly IMediator _mediator;
    public IsochronesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet()]
    public async Task<IActionResult> GetIsochronesGeoJson()
    {
        var result = await _mediator.Send(new GetIsochronesQuery());
        return Ok(result);
    }

    [HttpGet("Blob")]
    public async Task<IActionResult> GetBlob()
    {
        var result = await _mediator.Send(new GetBlobQuery());
        return Ok(result);
    }
}