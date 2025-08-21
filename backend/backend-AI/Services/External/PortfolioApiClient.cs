using System.Text.Json;

namespace backend_AI.Services.External
{
    public interface IPortfolioApiClient
    {
        Task<string> GetAllPortfoliosDetailedJsonAsync(CancellationToken cancellationToken = default);
        Task<System.Text.Json.JsonElement?> GetPortfolioByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<string> GetAllPortfoliosBasicJsonAsync(CancellationToken cancellationToken = default);
    }

    public class PortfolioApiClient : IPortfolioApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PortfolioApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PortfolioApiClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PortfolioApiClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetAllPortfoliosDetailedJsonAsync(CancellationToken cancellationToken = default)
        {
            var portfolioBaseUrl = Environment.GetEnvironmentVariable("PORTFOLIO_SERVICE_URL")
                                   ?? throw new InvalidOperationException("PORTFOLIO_SERVICE_URL environment variable is not set");

            var url = new Uri(new Uri(portfolioBaseUrl), "/api/portfolio/detailed-all");
            _logger.LogInformation("AI: Fetching portfolios from {Url}", url);
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Forward Bearer token from incoming request to portfolio service
            var incomingAuth = _httpContextAccessor.HttpContext?.Request?.Headers?.Authorization.ToString();
            if (!string.IsNullOrEmpty(incomingAuth) && incomingAuth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = incomingAuth.Substring(7).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("AI: Forwarding bearer token to portfolio service (len={Len})", token.Length);
                }
            }
            else
            {
                _logger.LogWarning("AI: No bearer token found on incoming request to forward to portfolio service");
            }

            using var httpClient = _httpClientFactory.CreateClient("PortfolioService");
            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async Task<System.Text.Json.JsonElement?> GetPortfolioByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var portfolioBaseUrl = Environment.GetEnvironmentVariable("PORTFOLIO_SERVICE_URL")
                                   ?? _configuration["PortfolioService:BaseUrl"]
                                   ?? "http://localhost:5201";

            var url = new Uri(new Uri(portfolioBaseUrl), $"/api/portfolio/{id}");
            _logger.LogInformation("AI: Fetching portfolio by id from {Url}", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var incomingAuth = _httpContextAccessor.HttpContext?.Request?.Headers?.Authorization.ToString();
            if (!string.IsNullOrEmpty(incomingAuth) && incomingAuth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = incomingAuth.Substring(7).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            using var httpClient = _httpClientFactory.CreateClient("PortfolioService");
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("AI: Portfolio {Id} not found", id);
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI: Error fetching portfolio {Id}: {Status} {Body}", id, (int)response.StatusCode, body);
                return null;
            }
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }

        public async Task<string> GetAllPortfoliosBasicJsonAsync(CancellationToken cancellationToken = default)
        {
            var portfolioBaseUrl = Environment.GetEnvironmentVariable("PORTFOLIO_SERVICE_URL")
                                   ?? _configuration["PortfolioService:BaseUrl"]
                                   ?? "http://localhost:5201";

            var url = new Uri(new Uri(portfolioBaseUrl), "/api/portfolio");
            _logger.LogInformation("AI: Fetching basic portfolios from {Url}", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var incomingAuth = _httpContextAccessor.HttpContext?.Request?.Headers?.Authorization.ToString();
            if (!string.IsNullOrEmpty(incomingAuth) && incomingAuth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = incomingAuth.Substring(7).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            using var httpClient = _httpClientFactory.CreateClient("PortfolioService");
            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}


