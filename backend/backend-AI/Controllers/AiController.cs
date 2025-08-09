using backend_AI.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace backend_AI.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;
        private readonly ILogger<AiController> _logger;

        public AiController(IAiChatService aiChatService, ILogger<AiController> logger)
        {
            _aiChatService = aiChatService ?? throw new ArgumentNullException(nameof(aiChatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("generate")]
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
    }
}


