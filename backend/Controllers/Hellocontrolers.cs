using Microsoft.AspNetCore.Mvc;

namespace RMZeitkarte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "Hello from .NET API!" });
    }
}
