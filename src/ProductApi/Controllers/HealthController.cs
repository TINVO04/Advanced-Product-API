using Microsoft.AspNetCore.Mvc;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var response = new
        {
            status = "Healthy",
            message = "Product API is running.",
            timestampUtc = DateTime.UtcNow
        };

        return Ok(response);
    }
}
