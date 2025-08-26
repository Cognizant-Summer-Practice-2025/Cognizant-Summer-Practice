using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Deployment.Request;
using backend_portfolio.DTO.Deployment.Response;
using backend_portfolio.Config;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace backend_portfolio.Services
{
    /// <summary>
    /// Service for deploying portfolios to Vercel
    /// </summary>
    public class VercelDeploymentService : IVercelDeploymentService
    {
        private readonly ILogger<VercelDeploymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _vercelToken;
        private readonly string _vercelApiUrl;

        public VercelDeploymentService(
            ILogger<VercelDeploymentService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient(HttpClientConfiguration.VercelApiClientName);
            _vercelToken = _configuration["Vercel:Token"] ?? throw new InvalidOperationException("Vercel token not configured");
            _vercelApiUrl = _configuration["Vercel:ApiUrl"] ?? "https://api.vercel.com";
        }

        public async Task<PortfolioDeploymentResponse> DeployPortfolioAsync(PortfolioDeploymentRequest request)
        {
            try
            {
                _logger.LogInformation("Starting portfolio deployment for user {UserId}", request.UserId);

                // Create Vercel deployment config
                var vercelConfig = new VercelDeploymentConfig
                {
                    ProjectName = request.ProjectName ?? GenerateProjectName(request.UserId),
                    CustomDomain = request.CustomDomain,
                    Framework = "nextjs",
                    BuildCommand = "npm run build",
                    InstallCommand = "npm install",
                    OutputDirectory = ".next"
                };

                // This would typically get the files from template extraction service
                var files = new Dictionary<string, string>();

                // Deploy to Vercel
                var vercelInfo = await CreateVercelProjectAsync(vercelConfig.ProjectName, files, vercelConfig);

                var deploymentResponse = new PortfolioDeploymentResponse
                {
                    DeploymentId = Guid.NewGuid(),
                    PortfolioId = request.PortfolioId ?? Guid.NewGuid(),
                    UserId = request.UserId,
                    ProjectName = vercelConfig.ProjectName,
                    DeploymentUrl = vercelInfo.VercelUrl,
                    CustomDomain = request.CustomDomain,
                    Status = DeploymentStatus.Completed,
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    VercelInfo = vercelInfo
                };

                _logger.LogInformation("Portfolio deployment completed: {DeploymentUrl}", deploymentResponse.DeploymentUrl);
                return deploymentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying portfolio for user {UserId}", request.UserId);
                
                return new PortfolioDeploymentResponse
                {
                    DeploymentId = Guid.NewGuid(),
                    PortfolioId = request.PortfolioId ?? Guid.NewGuid(),
                    UserId = request.UserId,
                    ProjectName = request.ProjectName ?? "failed-deployment",
                    DeploymentUrl = "",
                    Status = DeploymentStatus.Failed,
                    ErrorMessage = ex.Message,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<VercelDeploymentInfo> CreateVercelProjectAsync(
            string projectName, 
            Dictionary<string, string> files, 
            VercelDeploymentConfig config)
        {
            try
            {
                _logger.LogInformation("Creating Vercel project: {ProjectName}", projectName);

                // Create deployment payload using correct Vercel API format
                var deploymentPayload = new
                {
                    name = projectName,
                    files = files.Select(f => new
                    {
                        file = f.Key,
                        data = f.Value, // For text files, send as string
                        encoding = "utf-8" // Vercel requires "utf-8" with hyphen
                    }).ToArray(),
                    projectSettings = new
                    {
                        framework = config.Framework,
                        buildCommand = config.BuildCommand,
                        installCommand = config.InstallCommand,
                        outputDirectory = config.OutputDirectory
                    },
                    target = "production"
                };

                var jsonContent = JsonSerializer.Serialize(deploymentPayload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                _logger.LogInformation("Vercel deployment payload: {PayloadSize} bytes", jsonContent.Length);
                _logger.LogDebug("Vercel deployment payload content: {Payload}", jsonContent);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Create deployment
                var response = await _httpClient.PostAsync("/v13/deployments", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Vercel deployment failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    throw new Exception($"Vercel deployment failed: {response.StatusCode}");
                }

                var deploymentResult = JsonSerializer.Deserialize<VercelDeploymentResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (deploymentResult == null)
                {
                    throw new Exception("Failed to parse Vercel deployment response");
                }

                // Wait for deployment to complete
                await WaitForDeploymentCompletion(deploymentResult.Id);

                var vercelInfo = new VercelDeploymentInfo
                {
                    VercelProjectId = projectName,
                    VercelDeploymentId = deploymentResult.Id,
                    VercelUrl = deploymentResult.Url,
                    VercelAlias = deploymentResult.Alias?.FirstOrDefault(),
                    BuildDuration = TimeSpan.FromSeconds(deploymentResult.BuildDuration ?? 0),
                    VercelRegion = deploymentResult.Region ?? "iad1"
                };

                // Set custom domain if specified
                if (!string.IsNullOrEmpty(config.CustomDomain))
                {
                    await SetCustomDomainAsync(projectName, config.CustomDomain);
                    vercelInfo.VercelAlias = config.CustomDomain;
                }

                _logger.LogInformation("Vercel project created successfully: {Url}", vercelInfo.VercelUrl);
                return vercelInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Vercel project {ProjectName}", projectName);
                throw;
            }
        }

        public async Task<DeploymentStatus> GetDeploymentStatusAsync(string deploymentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v13/deployments/{deploymentId}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get deployment status: {StatusCode} - {Content}", response.StatusCode, content);
                    return DeploymentStatus.Failed;
                }

                var deployment = JsonSerializer.Deserialize<VercelDeploymentResult>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return deployment?.State switch
                {
                    "BUILDING" => DeploymentStatus.Building,
                    "READY" => DeploymentStatus.Completed,
                    "ERROR" => DeploymentStatus.Failed,
                    "CANCELED" => DeploymentStatus.Cancelled,
                    _ => DeploymentStatus.InProgress
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployment status for {DeploymentId}", deploymentId);
                return DeploymentStatus.Failed;
            }
        }

        public async Task<List<string>> GetDeploymentLogsAsync(string deploymentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v13/deployments/{deploymentId}/events");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new List<string> { $"Failed to fetch logs: {response.StatusCode}" };
                }

                var events = JsonSerializer.Deserialize<VercelEventResult>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return events?.Events?.Select(e => $"[{e.Created}] {e.Text}").ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployment logs for {DeploymentId}", deploymentId);
                return new List<string> { $"Error fetching logs: {ex.Message}" };
            }
        }

        public async Task<bool> DeleteDeploymentAsync(string deploymentId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/v13/deployments/{deploymentId}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Deployment {DeploymentId} deleted successfully", deploymentId);
                    return true;
                }

                _logger.LogWarning("Failed to delete deployment {DeploymentId}: {StatusCode}", deploymentId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting deployment {DeploymentId}", deploymentId);
                return false;
            }
        }

        public async Task<bool> SetCustomDomainAsync(string projectId, string domain)
        {
            try
            {
                var domainPayload = new
                {
                    name = domain
                };

                var jsonContent = JsonSerializer.Serialize(domainPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/v9/projects/{projectId}/domains", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Custom domain {Domain} set for project {ProjectId}", domain, projectId);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to set custom domain {Domain}: {StatusCode} - {Content}", domain, response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting custom domain {Domain} for project {ProjectId}", domain, projectId);
                return false;
            }
        }

        #region Private Helper Methods

        private string GenerateProjectName(Guid userId)
        {
            // Vercel project names must be lowercase alphanumeric with hyphens
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var userPrefix = userId.ToString()[..8].ToLowerInvariant();
            return $"portfolio-{userPrefix}-{timestamp}";
        }

        private async Task WaitForDeploymentCompletion(string deploymentId, int maxWaitTimeSeconds = 300)
        {
            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(maxWaitTimeSeconds);

            while (DateTime.UtcNow - startTime < timeout)
            {
                var status = await GetDeploymentStatusAsync(deploymentId);
                
                if (status == DeploymentStatus.Completed || status == DeploymentStatus.Failed)
                {
                    return;
                }

                await Task.Delay(5000); // Wait 5 seconds before checking again
            }

            _logger.LogWarning("Deployment {DeploymentId} did not complete within timeout", deploymentId);
        }

        #endregion

        #region DTOs for Vercel API responses

        private class VercelDeploymentResult
        {
            public string Id { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string? State { get; set; }
            public string? Region { get; set; }
            public double? BuildDuration { get; set; }
            public List<string>? Alias { get; set; }
        }

        private class VercelEventResult
        {
            public List<VercelEvent>? Events { get; set; }
        }

        private class VercelEvent
        {
            public string Text { get; set; } = string.Empty;
            public DateTime Created { get; set; }
        }

        #endregion
    }
}
