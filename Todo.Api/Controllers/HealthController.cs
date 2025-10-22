using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;


[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
  [HttpGet]
  public IActionResult Get() => Ok(new { status = "ok", timestamp = DateTime.UtcNow });
}