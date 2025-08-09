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

        public PortfolioApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<PortfolioApiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetAllPortfoliosDetailedJsonAsync(CancellationToken cancellationToken = default)
        {
            var portfolioBaseUrl = Environment.GetEnvironmentVariable("PORTFOLIO_SERVICE_URL")
                                   ?? _configuration["PortfolioService:BaseUrl"]
                                   ?? "http://localhost:5201";

            var url = new Uri(new Uri(portfolioBaseUrl), "/api/portfolio/detailed-all");
            _logger.LogInformation("AI: Fetching portfolios from {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}


