using Microsoft.AspNetCore.Mvc;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "SEVPMS.Api",
            status = "ok",
            utc = DateTime.UtcNow
        });
    }
}
