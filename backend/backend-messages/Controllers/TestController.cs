using Microsoft.AspNetCore.Mvc;

namespace backend_messages.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { 
                message = "Messages API is working!", 
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        [HttpGet("cors")]
        public IActionResult TestCors()
        {
            Response.Headers.Append("X-Test-Header", "CORS is working");
            return Ok(new { 
                message = "CORS test successful",
                origin = Request.Headers["Origin"].FirstOrDefault(),
                timestamp = DateTime.UtcNow
            });
        }
    }
} 