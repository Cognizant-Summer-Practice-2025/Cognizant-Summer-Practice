using System.Globalization;
using System.Linq;
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
            // Minimal payload requested: only model and messages per OpenRouter docs

            var payload = new Dictionary<string, object>
            {
                ["model"] = model,
                ["messages"] = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["role"] = "user",
                        ["content"] = userPrompt
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation("AI: OpenRouter request model={Model}, bodyLen={Len}", model, json.Length);
            _logger.LogInformation("AI: BaseUrl={BaseUrl}", baseUrl);
            _logger.LogInformation("AI: Prompt preview: {Preview}", Truncate(userPrompt, 200));
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("AI: OPENROUTER_API_KEY is not set");
            }
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(apiKey))
            {
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }
            // Match minimal example: do not add extra headers

            _logger.LogInformation("AI: Sending request to OpenRouter model {Model}", model);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var requestId = response.Headers.TryGetValues("x-request-id", out var idValues) ? (idValues.FirstOrDefault() ?? string.Empty) : string.Empty;
            _logger.LogInformation("AI: OpenRouter responded status={Status} requestId={RequestId}", (int)response.StatusCode, requestId);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI: OpenRouter returned {StatusCode}: {Body}", response.StatusCode, error);
                throw new InvalidOperationException($"OpenRouter error: {response.StatusCode} - {error}");
            }

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("AI: OpenRouter raw response (truncated): {Body}", Truncate(responseText, 1000));
            using var doc = JsonDocument.Parse(responseText);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("finish_reason", out var finish))
                {
                    _logger.LogInformation("AI: finish_reason={FinishReason}", finish.GetString());
                }
                if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var content))
                {
                    var text = content.GetString() ?? string.Empty;
                    _logger.LogInformation("AI: Extracted content length={Len}", text?.Length ?? 0);
                    if (root.TryGetProperty("usage", out var usage))
                    {
                        var promptTokens = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : -1;
                        var completionTokens = usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : -1;
                        var totalTokens = usage.TryGetProperty("total_tokens", out var tt) ? tt.GetInt32() : -1;
                        _logger.LogInformation("AI: Usage tokens prompt={Prompt} completion={Completion} total={Total}", promptTokens, completionTokens, totalTokens);
                    }
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        _logger.LogInformation("AI: Content is empty string");
                    }
                    return text;
                }
                _logger.LogInformation("AI: Response missing message/content in first choice");
            }
            else if (root.TryGetProperty("choices", out var emptyChoices))
            {
                _logger.LogInformation("AI: choices array is empty");
            }
            else
            {
                _logger.LogInformation("AI: Response missing 'choices' property");
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
            // generation controls (configurable)
            var maxTokens = 512; // sensible default
            var maxTokensEnv = Environment.GetEnvironmentVariable("OPENROUTER_MAX_TOKENS");
            var maxTokensCfg = _configuration["OpenRouter:MaxTokens"];
            if (int.TryParse(maxTokensEnv, out var envMax) && envMax > 0)
            {
                maxTokens = envMax;
            }
            else if (int.TryParse(maxTokensCfg, out var cfgMax) && cfgMax > 0)
            {
                maxTokens = cfgMax;
            }
            double? temperature = null;
            var tempEnv = Environment.GetEnvironmentVariable("OPENROUTER_TEMPERATURE");
            var tempCfg = _configuration["OpenRouter:Temperature"];
            if (double.TryParse(tempEnv, NumberStyles.Float, CultureInfo.InvariantCulture, out var envTemp))
            {
                temperature = envTemp;
            }
            else if (double.TryParse(tempCfg, NumberStyles.Float, CultureInfo.InvariantCulture, out var cfgTemp))
            {
                temperature = cfgTemp;
            }

            var payload = new Dictionary<string, object>
            {
                ["model"] = model,
                ["messages"] = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["role"] = "user",
                        ["content"] = prompt
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation("AI: OpenRouter request (prompt) model={Model}, bodyLen={Len}", model, json.Length);
            _logger.LogInformation("AI: Prompt preview: {Preview}", Truncate(prompt, 200));
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(apiKey))
            {
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }
            // Match minimal example: do not add extra headers
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var requestId2 = response.Headers.TryGetValues("x-request-id", out var idValues2) ? (idValues2.FirstOrDefault() ?? string.Empty) : string.Empty;
            _logger.LogInformation("AI: OpenRouter responded status={Status} requestId={RequestId}", (int)response.StatusCode, requestId2);
            if (!response.IsSuccessStatusCode)
            {
                var error2 = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI: OpenRouter returned {StatusCode}: {Body}", response.StatusCode, error2);
                throw new InvalidOperationException($"OpenRouter error: {response.StatusCode} - {error2}");
            }
            var responseText2 = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("AI: OpenRouter raw response (truncated): {Body}", Truncate(responseText2, 1000));
            using var doc = JsonDocument.Parse(responseText2);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("finish_reason", out var finish2))
                {
                    _logger.LogInformation("AI: finish_reason={FinishReason}", finish2.GetString());
                }
                if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var content))
                {
                    var text = content.GetString() ?? string.Empty;
                    _logger.LogInformation("AI: Extracted content length={Len}", text?.Length ?? 0);
                    if (root.TryGetProperty("usage", out var usage))
                    {
                        var promptTokens = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : -1;
                        var completionTokens = usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : -1;
                        var totalTokens = usage.TryGetProperty("total_tokens", out var tt) ? tt.GetInt32() : -1;
                        _logger.LogInformation("AI: Usage tokens prompt={Prompt} completion={Completion} total={Total}", promptTokens, completionTokens, totalTokens);
                    }
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        _logger.LogInformation("AI: Content is empty string");
                    }
                    return text;
                }
                _logger.LogInformation("AI: Response missing message/content in first choice");
            }
            else if (root.TryGetProperty("choices", out var emptyChoices2))
            {
                _logger.LogInformation("AI: choices array is empty");
            }
            else
            {
                _logger.LogInformation("AI: Response missing 'choices' property");
            }
            return string.Empty;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
            return value.Substring(0, maxLength) + "...";
        }
    }
}


