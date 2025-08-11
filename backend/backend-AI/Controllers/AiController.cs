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
        private readonly Services.Abstractions.IPortfolioRankingService _rankingService;
        private readonly ILogger<AiController> _logger;

        public AiController(IAiChatService aiChatService, Services.External.IPortfolioApiClient portfolioApiClient, Services.Abstractions.IPortfolioRankingService rankingService, ILogger<AiController> logger)
        {
            _aiChatService = aiChatService ?? throw new ArgumentNullException(nameof(aiChatService));
            _portfolioApiClient = portfolioApiClient ?? throw new ArgumentNullException(nameof(portfolioApiClient));
            _rankingService = rankingService ?? throw new ArgumentNullException(nameof(rankingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("generate")]
        public async Task<IActionResult> Generate(CancellationToken cancellationToken)
        {
            try
            {
                // No body required; service will read env vars
                var text = await _aiChatService.GenerateAsync(cancellationToken: cancellationToken);
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogInformation("AI Controller: generation returned empty content");
                }
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
                // Use detailed data for ranking input (as before)
                var portfoliosJson = await _portfolioApiClient.GetAllPortfoliosDetailedJsonAsync(cancellationToken);
                var top = _rankingService.SelectTopCandidates(portfoliosJson, topN: 24);
                _logger.LogInformation("AI: Ranking returned {Count} top candidates", top.Count);
                var compact = System.Text.Json.JsonSerializer.Serialize(top.Select(t => new { id = t.Id, scores = new { t.ExperienceScore, t.SkillsScore, t.BlogScore, t.BioScore, t.ProjectQualityScore, t.TotalScore } }));
                _logger.LogInformation("AI: Payload to model length={Len}", compact.Length + basePrompt.Length);
                // Compose prompt with only top candidates and their scores plus a note to select only id
                var prompt = $"{basePrompt}\nTop candidates (with precomputed scores):\n{compact}";
                var text = await _aiChatService.GenerateWithPromptAsync(prompt, cancellationToken);
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogInformation("AI Controller: best-portfolio generation returned empty content");
                    return Ok(new { response = text });
                }

                // Expecting 10 comma-separated UUIDs
                var ids = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                              .Select(s => s.Trim())
                              .Where(s => s.Length == 36)
                              .Take(10)
                              .ToList();
                _logger.LogInformation("AI Controller: received {Count} ids from model", ids.Count);

                // For final items, fetch only the basic portfolio table (not comprehensive details)
                var allBasic = await _portfolioApiClient.GetAllPortfoliosBasicJsonAsync(cancellationToken);
                using var basicDoc = System.Text.Json.JsonDocument.Parse(allBasic);
                var root = basicDoc.RootElement;
                var wanted = new HashSet<Guid>(ids
                    .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty));
                var idOrder = ids
                    .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToList();

                var map = new Dictionary<Guid, System.Text.Json.JsonElement>();
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var el in root.EnumerateArray())
                    {
                        if (el.TryGetProperty("id", out var idProp) && idProp.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            if (Guid.TryParse(idProp.GetString(), out var gid) && wanted.Contains(gid))
                            {
                                map[gid] = el.Clone();
                            }
                        }
                    }
                }

                var results = new List<System.Text.Json.JsonElement>();
                foreach (var gid in idOrder)
                {
                    if (map.TryGetValue(gid, out var el))
                    {
                        results.Add(el);
                    }
                }
                return Ok(new { response = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI: Error generating best portfolio");
                return StatusCode(502, new { error = ex.Message });
            }
        }
    }
}


