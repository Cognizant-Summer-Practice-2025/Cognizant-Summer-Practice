using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.ImageUpload.Response;
using backend_portfolio.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioQueryService _portfolioQueryService;
        private readonly IPortfolioCommandService _portfolioCommandService;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            IPortfolioQueryService portfolioQueryService,
            IPortfolioCommandService portfolioCommandService,
            ILogger<PortfolioController> logger)
        {
            _portfolioQueryService = portfolioQueryService;
            _portfolioCommandService = portfolioCommandService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPortfolios()
        {
            try
            {
                var portfolios = await _portfolioQueryService.GetAllPortfoliosAsync();
                return Ok(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all portfolios");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPortfolioById(Guid id)
        {
            try
            {
                var portfolio = await _portfolioQueryService.GetPortfolioByIdAsync(id);
                if (portfolio == null)
                {
                    return NotFound($"Portfolio with ID {id} not found.");
                }
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting portfolio by ID: {PortfolioId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPortfoliosByUserId(Guid userId)
        {
            try
            {
                var portfolios = await _portfolioQueryService.GetPortfoliosByUserIdAsync(userId);
                return Ok(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting portfolios for user: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePortfolio([FromBody] PortfolioCreateRequest request)
        {
            try
            {
                var portfolio = await _portfolioCommandService.CreatePortfolioAsync(request);
                return CreatedAtAction(nameof(GetPortfolioById), new { id = portfolio.Id }, portfolio);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating portfolio");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating portfolio");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create-and-get-id")]
        public async Task<IActionResult> CreatePortfolioAndGetId([FromBody] PortfolioCreateRequest request)
        {
            try
            {
                var portfolio = await _portfolioCommandService.CreatePortfolioAndGetIdAsync(request);
                return Ok(new { portfolioId = portfolio.Id });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating portfolio");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating portfolio and getting ID");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{portfolioId}/save-content")]
        public async Task<IActionResult> SavePortfolioContent(Guid portfolioId, [FromBody] BulkPortfolioContentRequest request)
        {
            try
            {
                var response = await _portfolioCommandService.SavePortfolioContentAsync(portfolioId, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving portfolio content for portfolio: {PortfolioId}", portfolioId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePortfolio(Guid id, [FromBody] PortfolioUpdateRequest request)
        {
            try
            {
                var portfolio = await _portfolioCommandService.UpdatePortfolioAsync(id, request);
                if (portfolio == null)
                    return NotFound($"Portfolio with ID {id} not found.");

                return Ok(portfolio);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating portfolio: {PortfolioId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating portfolio: {PortfolioId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            try
            {
                var result = await _portfolioCommandService.DeletePortfolioAsync(id);
                if (!result)
                    return NotFound($"Portfolio with ID {id} not found.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting portfolio: {PortfolioId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("published")]
        public async Task<IActionResult> GetPublishedPortfolios()
        {
            try
            {
                var portfolios = await _portfolioQueryService.GetPublishedPortfoliosAsync();
                return Ok(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting published portfolios");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("home-page-cards")]
        public async Task<IActionResult> GetPortfoliosForHomePage()
        {
            try
            {
                var portfolioCards = await _portfolioQueryService.GetPortfoliosForHomePageAsync();
                return Ok(portfolioCards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting portfolios for home page");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementViewCount(Guid id)
        {
            try
            {
                var result = await _portfolioCommandService.IncrementViewCountAsync(id);
                if (!result)
                    return NotFound($"Portfolio with ID {id} not found.");
                return Ok(new { message = "View count incremented successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while incrementing view count for portfolio: {PortfolioId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> IncrementLikeCount(Guid id)
        {
            try
            {
                var result = await _portfolioCommandService.IncrementLikeCountAsync(id);
                if (!result)
                    return NotFound($"Portfolio with ID {id} not found.");
                return Ok(new { message = "Like count incremented successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while incrementing like count for portfolio: {PortfolioId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/unlike")]
        public async Task<IActionResult> DecrementLikeCount(Guid id)
        {
            try
            {
                var result = await _portfolioCommandService.DecrementLikeCountAsync(id);
                if (!result)
                    return NotFound($"Portfolio with ID {id} not found.");
                return Ok(new { message = "Like count decremented successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while decrementing like count for portfolio: {PortfolioId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/comprehensive")]
        public async Task<IActionResult> GetUserPortfolioComprehensive(Guid userId)
        {
            try
            {
                var response = await _portfolioQueryService.GetUserPortfolioComprehensiveAsync(userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting comprehensive portfolio data for user: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
