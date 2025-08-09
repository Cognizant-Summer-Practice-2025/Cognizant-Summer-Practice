using System.Text;
using System.Text.Json;
using backend_AI.Services.Abstractions;

namespace backend_AI.Services
{
    /// <summary>
    /// Service for calling Ollama's HTTP API to generate text.
    /// </summary>
    public class AiChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiChatService> _logger;

        public AiChatService(HttpClient httpClient, IConfiguration configuration, ILogger<AiChatService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
        {
            var baseUrl = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL")
                          ?? _configuration["Ollama:BaseUrl"]
                          ?? "http://localhost:11434";
            var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL")
                        ?? Environment.GetEnvironmentVariable("OLLAMA_MODEL")
                        ?? _configuration["Ollama:DefaultModel"]
                        ?? "llama3";
            var prompt = Environment.GetEnvironmentVariable("OLLAMA_PROMPT")
                         ?? Environment.GetEnvironmentVariable("OLLAMA_PROMPT")
                         ?? "Hello!";

            // Ollama /api/generate expects: { model, prompt, stream(false), options{...} }
            var payload = new
            {
                model,
                prompt = prompt,
                stream = false,
                // no per-request options when using env-only mode
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogInformation("AI: Sending request to Ollama at {BaseUrl} with model {Model}", baseUrl, model);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var start = DateTime.UtcNow;
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var durationMs = (DateTime.UtcNow - start).TotalMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI: Ollama returned {StatusCode}: {Body}", response.StatusCode, error);
                throw new InvalidOperationException($"Ollama error: {response.StatusCode} - {error}");
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var root = doc.RootElement;
            // Non-stream response schema: { model, created_at, response, done, ... , eval_count, total_duration }
            var responseText = root.TryGetProperty("response", out var r) ? r.GetString() ?? string.Empty : string.Empty;
            return responseText;
        }
    }
}


