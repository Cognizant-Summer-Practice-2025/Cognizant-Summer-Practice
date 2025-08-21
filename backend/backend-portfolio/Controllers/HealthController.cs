using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
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
                service = "backend-portfolio",
                timestamp = DateTime.UtcNow
            });
        }
    }
}


