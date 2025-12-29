using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    //     [HttpGet]
    // public IActionResult GetIsochrones()
    // {

    //     return Ok(new { Message = "Hello from .NET API!" });
    // }

    [HttpGet("raw")]
    public async Task<IActionResult> GetRawData()
    {
        var result = await _mediator.Send(new GetAllRawQuery());
        return Ok(result);
    }
}