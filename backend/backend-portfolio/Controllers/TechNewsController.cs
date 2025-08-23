using System;
using System.Threading.Tasks;
using backend_portfolio.DTO.TechNews;
using backend_portfolio.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace backend_portfolio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechNewsController : ControllerBase
    {
        private readonly ITechNewsSummaryService _techNewsService;
        private readonly ILogger<TechNewsController> _logger;

        public TechNewsController(
            ITechNewsSummaryService techNewsService,
            ILogger<TechNewsController> logger)
        {
            _techNewsService = techNewsService ?? throw new ArgumentNullException(nameof(techNewsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get the latest tech news summary (called by backend-AI)
        /// </summary>
        /// <returns>The latest tech news summary or null if none exists</returns>
        [HttpGet]
        public async Task<ActionResult> GetLatest()
        {
            try
            {
                var summary = await _techNewsService.GetLatestAsync();
                
                if (summary == null)
                {
                    return Ok(new { Summary = "No tech news for now" });
                }

                var response = new { Summary = summary.Summary };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tech news summary");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create or update tech news summary (called by backend-AI from AirFlow)
        /// </summary>
        /// <param name="request">The tech news summary request</param>
        /// <returns>Success message</returns>
        [HttpPost]
        public async Task<ActionResult> PostSummary([FromBody] TechNewsSummaryRequestDto request)
        {
            try
            {
                // Check if this is a service-to-service call from backend-AI
                var isServiceToServiceCall = IsServiceToServiceCall();
                
                if (!isServiceToServiceCall)
                {
                    // For external calls (like direct AirFlow calls), require AIRFLOW_SECRET
                    var airflowSecret = Environment.GetEnvironmentVariable("AIRFLOW_SECRET");
                    if (string.IsNullOrEmpty(airflowSecret))
                    {
                        _logger.LogError("AIRFLOW_SECRET environment variable is not configured");
                        return StatusCode(503, new { error = "Airflow secret not configured on server" });
                    }

                    var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                    var xAirflowSecretHeader = Request.Headers["X-Airflow-Secret"].FirstOrDefault();

                    var isAuthorized = false;
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeader.Substring("Bearer ".Length).Trim();
                        if (token == airflowSecret)
                        {
                            isAuthorized = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(xAirflowSecretHeader) && xAirflowSecretHeader == airflowSecret)
                    {
                        isAuthorized = true;
                    }

                    if (!isAuthorized)
                    {
                        _logger.LogWarning("Unauthorized attempt to post tech news summary");
                        return Unauthorized(new { error = "Unauthorized: Invalid or missing Airflow secret" });
                    }
                }

                if (request == null)
                {
                    _logger.LogWarning("Request body is null");
                    return BadRequest(new { error = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Summary))
                {
                    _logger.LogWarning("Summary content is empty");
                    return BadRequest(new { error = "Summary content cannot be empty" });
                }

                var result = await _techNewsService.UpsertAsync(request);
                
                _logger.LogInformation("Tech news summary upserted successfully. ID: {Id}, SummaryLength: {Length}", 
                    result.Id, result.Summary.Length);

                return Ok(new { message = "Tech news summary received and stored successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid request: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting tech news summary");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Determines if the request is a service-to-service call from backend-AI
        /// </summary>
        private bool IsServiceToServiceCall()
        {
            // Check if the request is coming from localhost (service-to-service)
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            var isLocalhost = remoteIp?.Equals(System.Net.IPAddress.Loopback) == true || 
                             remoteIp?.Equals(System.Net.IPAddress.IPv6Loopback) == true ||
                             Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                             Request.Host.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

            // Also check if there's a specific header indicating it's from backend-AI
            var isFromBackendAI = Request.Headers.ContainsKey("X-Service-Name") && 
                                 Request.Headers["X-Service-Name"].ToString().Equals("backend-AI", StringComparison.OrdinalIgnoreCase);

            return isLocalhost || isFromBackendAI;
        }
    }
}
