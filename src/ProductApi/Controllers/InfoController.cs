using Microsoft.AspNetCore.Mvc;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var response = new
        {
            projectName = "ProductApi",
            version = "1.0.0",
            framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            description = "ASP.NET Core Web API for product management."
        };

        return Ok(response);
    }
}
