using Microsoft.AspNetCore.Mvc;

namespace BackendMessages.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "ok",
                service = "backend-messages",
                timestamp = DateTime.UtcNow
            });
        }
    }
}


