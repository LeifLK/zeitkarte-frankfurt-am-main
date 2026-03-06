using backend.Queries;
using Mediator;
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

    [HttpGet("{station:int}/{duration:int}")]
    public async Task<IActionResult> GetIsochronesByStationAndTime(int station, int duration)
    {
        var result = await _mediator.Send(new GetIsochronesByStationAndTimeQuery(station, duration));
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchStations([FromQuery] string q)
    {
        var result = await _mediator.Send(new SearchStationsQuery(q));
        return Ok(result);
    }
}