using Microsoft.AspNetCore.Mvc;
using Ciclo.Infrastructure.Data;

namespace Ciclo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy" });

    [HttpGet("db")]
    public async Task<IActionResult> GetDb(AppDbContext db)
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Ok(new { status = "Healthy", database = "Connected" })
            : StatusCode(503, new { status = "Unhealthy", database = "Disconnected" });
    }
}
