using System.Text;
using System.Text.Json;
using backend_AI.Services.Abstractions;

namespace backend_AI.Services
{
    /// <summary>
    /// Service for calling an AI provider (OpenRouter) to generate text.
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
            var baseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL")
                          ?? _configuration["OpenRouter:BaseUrl"]
                          ?? "https://openrouter.ai/api/v1/chat/completions";
            var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
                         ?? _configuration["OpenRouter:ApiKey"]
                         ?? string.Empty;
            var model = Environment.GetEnvironmentVariable("OPENROUTER_MODEL")
                        ?? _configuration["OpenRouter:Model"]
                        ?? "openai/gpt-oss-20b:free";
            var userPrompt = Environment.GetEnvironmentVariable("OPENROUTER_PROMPT")
                             ?? "Hello!";

            var payload = new
            {
                model,
                messages = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["role"] = "user",
                        ["content"] = userPrompt
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(apiKey))
            {
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }

            _logger.LogInformation("AI: Sending request to OpenRouter model {Model}", model);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI: OpenRouter returned {StatusCode}: {Body}", response.StatusCode, error);
                throw new InvalidOperationException($"OpenRouter error: {response.StatusCode} - {error}");
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var content))
                {
                    return content.GetString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        public async Task<string> GenerateWithPromptAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var baseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL")
                          ?? _configuration["OpenRouter:BaseUrl"]
                          ?? "https://openrouter.ai/api/v1/chat/completions";
            var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
                         ?? _configuration["OpenRouter:ApiKey"]
                         ?? string.Empty;
            var model = Environment.GetEnvironmentVariable("OPENROUTER_MODEL")
                        ?? _configuration["OpenRouter:Model"]
                        ?? "openai/gpt-oss-20b:free";

            var payload = new
            {
                model,
                messages = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["role"] = "user",
                        ["content"] = prompt
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(apiKey))
            {
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var content))
                {
                    return content.GetString() ?? string.Empty;
                }
            }
            return string.Empty;
        }
    }
}


