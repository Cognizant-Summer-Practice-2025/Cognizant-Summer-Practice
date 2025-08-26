using Microsoft.AspNetCore.Mvc;
using backend_user.Services.Abstractions;

namespace backend_user.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var healthStatus = await _healthCheckService.GetBasicHealthAsync();
            return Ok(healthStatus);
        }

        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            var databaseHealth = await _healthCheckService.GetDatabaseHealthAsync();
            
            // Check if the status indicates an error and return appropriate HTTP status
            var healthDict = databaseHealth as Dictionary<string, object>;
            if (healthDict != null && healthDict.ContainsKey("status") && healthDict["status"]?.ToString() == "error")
            {
                return StatusCode(503, databaseHealth);
            }
            
            return Ok(databaseHealth);
        }
    }
}


