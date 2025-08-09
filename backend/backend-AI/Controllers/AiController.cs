using backend_AI.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace backend_AI.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;
        private readonly Services.External.IPortfolioApiClient _portfolioApiClient;
        private readonly ILogger<AiController> _logger;

        public AiController(IAiChatService aiChatService, Services.External.IPortfolioApiClient portfolioApiClient, ILogger<AiController> logger)
        {
            _aiChatService = aiChatService ?? throw new ArgumentNullException(nameof(aiChatService));
            _portfolioApiClient = portfolioApiClient ?? throw new ArgumentNullException(nameof(portfolioApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("generate")]
        public async Task<IActionResult> Generate(CancellationToken cancellationToken)
        {
            try
            {
                // No body required; service will read env vars
                var text = await _aiChatService.GenerateAsync(cancellationToken: cancellationToken);
                return Ok(new { response = text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI: Error generating text");
                return StatusCode(502, new { error = ex.Message });
            }
        }

        [HttpGet("generate-best-portfolio")]
        public async Task<IActionResult> GenerateBestPortfolio(CancellationToken cancellationToken)
        {
            try
            {
                var basePrompt = Environment.GetEnvironmentVariable("BEST_PORTFOLIO_PROMPT")
                                 ?? "Given the following portfolios JSON, analyze and propose the best combined portfolio content.";
                var portfoliosJson = await _portfolioApiClient.GetAllPortfoliosDetailedJsonAsync(cancellationToken);
                // Compose prompt
                var prompt = $"{basePrompt}\n\nPortfolios:\n{portfoliosJson}";
                var text = await _aiChatService.GenerateWithPromptAsync(prompt, cancellationToken);
                return Ok(new { response = text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI: Error generating best portfolio");
                return StatusCode(502, new { error = ex.Message });
            }
        }
    }
}


