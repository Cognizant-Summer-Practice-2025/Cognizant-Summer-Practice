using System.Text.Json;

namespace backend_AI.Services.External
{
    public interface IPortfolioApiClient
    {
        Task<string> GetAllPortfoliosDetailedJsonAsync(CancellationToken cancellationToken = default);
    }

    public class PortfolioApiClient : IPortfolioApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PortfolioApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PortfolioApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<PortfolioApiClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetAllPortfoliosDetailedJsonAsync(CancellationToken cancellationToken = default)
        {
            var portfolioBaseUrl = Environment.GetEnvironmentVariable("PORTFOLIO_SERVICE_URL")
                                   ?? _configuration["PortfolioService:BaseUrl"]
                                   ?? "http://localhost:5201";

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

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}


