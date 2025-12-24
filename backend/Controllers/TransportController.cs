using Microsoft.AspNetCore.Mvc;
using RmvApiBackend.Services;
using System.Threading.Tasks;

namespace RmvApiBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // This will make the URL: /api/transport
    public class TransportController : ControllerBase
    {
        private readonly IRmvService _rmvService;
        private readonly ILogger<TransportController> _logger;

        // The IRmvService is "injected" by the framework
        public TransportController(IRmvService rmvService, ILogger<TransportController> logger)
        {
            _rmvService = rmvService;
            _logger = logger;
        }

        /// <summary>
        /// Searches for a location (station/stop) from the RMV API.
        /// Example URL: /api/transport/search?query=Frankfurt%20Hauptbahnhof
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchLocation([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("A 'query' parameter is required.");
            }

            _logger.LogInformation("Handling search request for: {Query}", query);

            var results = await _rmvService.FindLocationAsync(query);

            if (results == null)
            {
                // This could mean the API key is bad or the RMV service is down
                return StatusCode(500, "An error occurred while contacting the RMV service.");
            }

            return Ok(results);
        }
    }
}
