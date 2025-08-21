using Microsoft.AspNetCore.Mvc;

namespace backend_user.Controllers
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
                service = "backend-user",
                timestamp = DateTime.UtcNow
            });
        }
    }
}


