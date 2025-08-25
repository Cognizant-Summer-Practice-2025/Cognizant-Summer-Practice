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
        private readonly IAirflowAuthorizationService _airflowAuthorizationService;

        public TechNewsController(
            ITechNewsSummaryService techNewsService,
            IAirflowAuthorizationService airflowAuthorizationService,
            ILogger<TechNewsController> logger)
        {
            _techNewsService = techNewsService ?? throw new ArgumentNullException(nameof(techNewsService));
            _airflowAuthorizationService = airflowAuthorizationService ?? throw new ArgumentNullException(nameof(airflowAuthorizationService));
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
                var isServiceToServiceCall = _airflowAuthorizationService.IsServiceToServiceCall(HttpContext);
                
                if (!isServiceToServiceCall)
                {
                    if (!_airflowAuthorizationService.IsAuthorizedExternalCall(HttpContext))
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
        // Authorization logic moved into service
    }
}
