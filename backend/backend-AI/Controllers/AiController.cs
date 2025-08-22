using backend_AI.Services.Abstractions;
using backend_AI.DTO.Ai;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ITechNewsSummaryStore _techNewsStore;

        public AiController(IAiChatService aiChatService, Services.External.IPortfolioApiClient portfolioApiClient, Services.Abstractions.IPortfolioRankingService rankingService, ILogger<AiController> logger, ITechNewsSummaryStore techNewsStore)
        {
            _aiChatService = aiChatService ?? throw new ArgumentNullException(nameof(aiChatService));
            _portfolioApiClient = portfolioApiClient ?? throw new ArgumentNullException(nameof(portfolioApiClient));
            _rankingService = rankingService ?? throw new ArgumentNullException(nameof(rankingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _techNewsStore = techNewsStore ?? throw new ArgumentNullException(nameof(techNewsStore));
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
                
                // Check if we have enough portfolios for meaningful AI generation
                var PORTFOLIO_COUNT = 10;
                if (top.Count < PORTFOLIO_COUNT)
                {
                    _logger.LogWarning("AI: Insufficient portfolio data. Found {Count} portfolios, need at least 10", top.Count);
                    return BadRequest(new { error = "Insufficient portfolio data. The AI requires at least 10 portfolios in the database to generate meaningful recommendations. Please add more portfolios and try again." });
                }
                _logger.LogInformation("AI: Ranking returned {Count} top candidates", top.Count);
                var compact = System.Text.Json.JsonSerializer.Serialize(top.Select(t => new { id = t.Id, scores = new { t.ExperienceScore, t.SkillsScore, t.BlogScore, t.BioScore, t.ProjectQualityScore, t.TotalScore } }));
                _logger.LogInformation("AI: Payload to model length={Len}", compact.Length + basePrompt.Length);
                
                var prompt = $"{basePrompt}\nTop candidates (with precomputed scores):\n{compact}";
                var text = await _aiChatService.GenerateWithPromptAsync(prompt, cancellationToken);
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogInformation("AI Controller: best-portfolio generation returned empty content");
                    return Ok(new { response = text });
                }

                var ids = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                              .Select(s => s.Trim())
                              .Where(s => s.Length == 36)
                              .Take(10)
                              .ToList();
                _logger.LogInformation("AI Controller: received {Count} ids from model", ids.Count);

                var allBasic = await _portfolioApiClient.GetAllPortfoliosBasicJsonAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(allBasic))
                {
                    allBasic = portfoliosJson;
                }
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
                
                // Final check to ensure we have meaningful results
                if (results.Count == 0)
                {
                    _logger.LogWarning("AI: No valid portfolios could be matched from AI selection");
                    return BadRequest(new { error = "No valid portfolios could be processed. The AI requires at least 10 portfolios in the database to generate recommendations." });
                }
                
                _logger.LogInformation("AI: Returning {Count} portfolios", results.Count);
                return Ok(new { response = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI: Error generating best portfolio");
                return StatusCode(502, new { error = ex.Message });
            }
        }

        [HttpPost("tech-news")]
        [AllowAnonymous]
        public IActionResult UpsertTechNews([FromBody] TechNewsSummaryRequest request)
        {
            _logger.LogInformation("TechNews POST: Received request");
            var configuredSecret = Environment.GetEnvironmentVariable("AIRFLOW_SECRET");
            if (string.IsNullOrWhiteSpace(configuredSecret))
            {
                _logger.LogWarning("TechNews POST: AIRFLOW_SECRET is not configured");
                return StatusCode(503, new { error = "Service not configured" });
            }

            // Accept either Authorization: Bearer <secret> or X-Airflow-Secret: <secret>
            string? providedSecret = null;
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var header = authHeader.ToString();
                const string bearerPrefix = "Bearer ";
                if (header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    providedSecret = header.Substring(bearerPrefix.Length).Trim();
                }
            }
            if (string.IsNullOrEmpty(providedSecret) && Request.Headers.TryGetValue("X-Airflow-Secret", out var xHeader))
            {
                providedSecret = xHeader.ToString().Trim();
            }

            if (!string.Equals(providedSecret, configuredSecret, StringComparison.Ordinal))
            {
                _logger.LogWarning("TechNews POST: Secret auth failed (provided: {ProvidedLength} chars)", providedSecret?.Length ?? 0);
                return Unauthorized(new { error = "Unauthorized" });
            }
            if (request is null)
            {
                _logger.LogWarning("TechNews POST: Missing body");
                return BadRequest(new { error = "Request body is required" });
            }
            if (string.IsNullOrWhiteSpace(request.Summary))
            {
                _logger.LogWarning("TechNews POST: Missing summary");
                return BadRequest(new { error = "summary is required" });
            }

            var preview = request.Summary.Length > 120 ? request.Summary.Substring(0, 120) + "..." : request.Summary;
            _logger.LogInformation("TechNews POST: Upserting summaryLength={Length}, preview=\n{Preview}", request.Summary.Length, preview);
            _techNewsStore.SetSummary(request.Summary);
            _logger.LogInformation("Tech news summary updated");
            return Ok(new { status = "ok" });
        }

        [HttpGet("tech-news")]
        [AllowAnonymous]
        public IActionResult GetLatestTechNews()
        {
            _logger.LogInformation("TechNews GET: Fetching latest summary");
            var summary = _techNewsStore.Summary;
            var length = string.IsNullOrEmpty(summary) ? 0 : summary.Length;
            _logger.LogInformation("TechNews GET: summaryLength={Length}", length);

            if (string.IsNullOrWhiteSpace(summary))
            {
                _logger.LogInformation("TechNews GET: No summary available, returning default message");
                return Ok(new TechNewsSummaryResponse
                {
                    Summary = "No tech news for now"
                });
            }

            return Ok(new TechNewsSummaryResponse { Summary = summary });
        }
    }
}


