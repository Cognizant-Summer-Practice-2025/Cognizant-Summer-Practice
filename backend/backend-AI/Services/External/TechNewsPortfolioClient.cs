using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using backend_AI.DTO.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace backend_AI.Services.External
{
    public class TechNewsPortfolioClient : ITechNewsPortfolioClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TechNewsPortfolioClient> _logger;
        private readonly string _portfolioServiceUrl;

        public TechNewsPortfolioClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TechNewsPortfolioClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _portfolioServiceUrl = configuration["PORTFOLIO_SERVICE_URL"];
            if (string.IsNullOrEmpty(_portfolioServiceUrl))
            {
                throw new InvalidOperationException("PORTFOLIO_SERVICE_URL configuration is not set");
            }
        }

        public async Task<bool> UpsertSummaryAsync(TechNewsSummaryDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Add service identification header
                content.Headers.Add("X-Service-Name", "backend-AI");

                var response = await _httpClient.PostAsync($"{_portfolioServiceUrl}/api/technews", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to upsert tech news summary. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling portfolio service to upsert tech news summary");
                return false;
            }
        }

        public async Task<string?> GetLatestSummaryAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_portfolioServiceUrl}/api/technews");
                request.Headers.Add("X-Service-Name", "backend-AI");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to get tech news summary. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling portfolio service to get tech news summary");
                return null;
            }
        }
    }
}
