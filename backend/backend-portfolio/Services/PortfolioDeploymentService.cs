using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Deployment.Request;
using backend_portfolio.DTO.Deployment.Response;
using backend_portfolio.DTO.Portfolio.Response;

namespace backend_portfolio.Services
{
    /// <summary>
    /// Main service for orchestrating portfolio deployments
    /// </summary>
    public class PortfolioDeploymentService : IPortfolioDeploymentService
    {
        private readonly ILogger<PortfolioDeploymentService> _logger;
        private readonly ITemplateExtractionService _templateExtractionService;
        private readonly IVercelDeploymentService _vercelDeploymentService;
        private readonly IPortfolioQueryService _portfolioQueryService;
        private readonly ICacheService _cacheService;

        // In-memory storage for deployments (would typically use database)
        private readonly Dictionary<Guid, PortfolioDeploymentResponse> _deployments = new();
        private readonly Dictionary<Guid, List<DeploymentSummaryResponse>> _userDeployments = new();

        public PortfolioDeploymentService(
            ILogger<PortfolioDeploymentService> logger,
            ITemplateExtractionService templateExtractionService,
            IVercelDeploymentService vercelDeploymentService,
            IPortfolioQueryService portfolioQueryService,
            ICacheService cacheService)
        {
            _logger = logger;
            _templateExtractionService = templateExtractionService;
            _vercelDeploymentService = vercelDeploymentService;
            _portfolioQueryService = portfolioQueryService;
            _cacheService = cacheService;
        }

        public async Task<PortfolioDeploymentResponse> DeployUserPortfolioAsync(PortfolioDeploymentRequest request)
        {
            try
            {
                _logger.LogInformation("Starting portfolio deployment for user {UserId} with template {TemplateName}", 
                    request.UserId, request.TemplateName);

                // Step 1: Validate template
                if (!await _templateExtractionService.ValidateTemplateAsync(request.TemplateName))
                {
                    throw new ArgumentException($"Template '{request.TemplateName}' not found or not accessible");
                }

                // Step 2: Get user portfolio data
                var portfolioData = await GetUserPortfolioDataAsync(request.UserId, request.PortfolioId);

                // Step 3: Extract template
                var templateRequest = new TemplateExtractionRequest
                {
                    TemplateName = request.TemplateName,
                    IncludeStyles = true,
                    IncludeComponents = true,
                    MinifyCode = request.Environment == DeploymentEnvironment.Production
                };

                var templateData = await _templateExtractionService.ExtractTemplateAsync(templateRequest);

                // Step 4: Create project structure
                var projectFiles = await _templateExtractionService.CreateProjectStructureAsync(templateData, portfolioData);

                // Step 5: Create Vercel deployment config
                var vercelConfig = new VercelDeploymentConfig
                {
                    ProjectName = request.ProjectName ?? GenerateProjectName(request.UserId, request.TemplateName),
                    CustomDomain = request.CustomDomain,
                    Framework = "nextjs",
                    BuildCommand = "npm run build",
                    InstallCommand = "npm install",
                    OutputDirectory = ".next",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        { "NEXT_PUBLIC_TEMPLATE_NAME", request.TemplateName },
                        { "NEXT_PUBLIC_DEPLOYED_FROM", "Goalkeeper Platform" }
                    }
                };

                // Step 6: Deploy to Vercel
                var vercelInfo = await _vercelDeploymentService.CreateVercelProjectAsync(
                    vercelConfig.ProjectName, 
                    projectFiles, 
                    vercelConfig);

                // Step 7: Create deployment response
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

                // Step 8: Store deployment record
                _deployments[deploymentResponse.DeploymentId] = deploymentResponse;
                
                if (!_userDeployments.ContainsKey(request.UserId))
                {
                    _userDeployments[request.UserId] = new List<DeploymentSummaryResponse>();
                }

                _userDeployments[request.UserId].Add(new DeploymentSummaryResponse
                {
                    DeploymentId = deploymentResponse.DeploymentId,
                    PortfolioId = deploymentResponse.PortfolioId,
                    ProjectName = deploymentResponse.ProjectName,
                    DeploymentUrl = deploymentResponse.DeploymentUrl,
                    Status = deploymentResponse.Status,
                    CreatedAt = deploymentResponse.CreatedAt,
                    CompletedAt = deploymentResponse.CompletedAt
                });

                // Step 9: Cache the result
                await _cacheService.SetAsync($"deployment:{deploymentResponse.DeploymentId}", deploymentResponse, TimeSpan.FromHours(24));

                _logger.LogInformation("Portfolio deployment completed successfully: {DeploymentUrl}", deploymentResponse.DeploymentUrl);
                return deploymentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying portfolio for user {UserId}", request.UserId);
                
                var errorResponse = new PortfolioDeploymentResponse
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

                _deployments[errorResponse.DeploymentId] = errorResponse;
                return errorResponse;
            }
        }

        public async Task<PortfolioDeploymentResponse?> GetDeploymentStatusAsync(Guid deploymentId)
        {
            try
            {
                // First check cache
                var cachedDeployment = await _cacheService.GetAsync<PortfolioDeploymentResponse>($"deployment:{deploymentId}");
                if (cachedDeployment != null)
                {
                    return cachedDeployment;
                }

                // Check in-memory storage
                if (_deployments.TryGetValue(deploymentId, out var deployment))
                {
                    // If deployment is still in progress, check Vercel status
                    if (deployment.Status == DeploymentStatus.InProgress || deployment.Status == DeploymentStatus.Building)
                    {
                        if (deployment.VercelInfo != null)
                        {
                            var vercelStatus = await _vercelDeploymentService.GetDeploymentStatusAsync(
                                deployment.VercelInfo.VercelDeploymentId);
                            
                            deployment.Status = vercelStatus;
                            if (vercelStatus == DeploymentStatus.Completed || vercelStatus == DeploymentStatus.Failed)
                            {
                                deployment.CompletedAt = DateTime.UtcNow;
                            }

                            // Update cache
                            await _cacheService.SetAsync($"deployment:{deploymentId}", deployment, TimeSpan.FromHours(24));
                        }
                    }

                    return deployment;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployment status for {DeploymentId}", deploymentId);
                return null;
            }
        }

        public Task<List<DeploymentSummaryResponse>> GetUserDeploymentsAsync(Guid userId)
        {
            try
            {
                if (_userDeployments.TryGetValue(userId, out var deployments))
                {
                    return Task.FromResult(deployments.OrderByDescending(d => d.CreatedAt).ToList());
                }

                return Task.FromResult(new List<DeploymentSummaryResponse>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployments for user {UserId}", userId);
                return Task.FromResult(new List<DeploymentSummaryResponse>());
            }
        }

        public Task<List<DeploymentSummaryResponse>> GetPortfolioDeploymentsAsync(Guid portfolioId)
        {
            try
            {
                var portfolioDeployments = _deployments.Values
                    .Where(d => d.PortfolioId == portfolioId)
                    .Select(d => new DeploymentSummaryResponse
                    {
                        DeploymentId = d.DeploymentId,
                        PortfolioId = d.PortfolioId,
                        ProjectName = d.ProjectName,
                        DeploymentUrl = d.DeploymentUrl,
                        Status = d.Status,
                        CreatedAt = d.CreatedAt,
                        CompletedAt = d.CompletedAt
                    })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();

                return Task.FromResult(portfolioDeployments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployments for portfolio {PortfolioId}", portfolioId);
                return Task.FromResult(new List<DeploymentSummaryResponse>());
            }
        }

        public async Task<bool> CancelDeploymentAsync(Guid deploymentId)
        {
            try
            {
                if (_deployments.TryGetValue(deploymentId, out var deployment))
                {
                    if (deployment.Status == DeploymentStatus.InProgress || 
                        deployment.Status == DeploymentStatus.Pending ||
                        deployment.Status == DeploymentStatus.Building)
                    {
                        deployment.Status = DeploymentStatus.Cancelled;
                        deployment.CompletedAt = DateTime.UtcNow;

                        // Update cache
                        await _cacheService.SetAsync($"deployment:{deploymentId}", deployment, TimeSpan.FromHours(24));

                        _logger.LogInformation("Deployment {DeploymentId} cancelled", deploymentId);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling deployment {DeploymentId}", deploymentId);
                return false;
            }
        }

        public async Task<bool> DeleteDeploymentAsync(Guid deploymentId)
        {
            try
            {
                if (_deployments.TryGetValue(deploymentId, out var deployment))
                {
                    // Delete from Vercel if possible
                    if (deployment.VercelInfo != null)
                    {
                        await _vercelDeploymentService.DeleteDeploymentAsync(deployment.VercelInfo.VercelDeploymentId);
                    }

                    // Remove from storage
                    _deployments.Remove(deploymentId);

                    // Remove from user deployments
                    if (_userDeployments.TryGetValue(deployment.UserId, out var userDeployments))
                    {
                        userDeployments.RemoveAll(d => d.DeploymentId == deploymentId);
                    }

                    // Remove from cache
                    await _cacheService.RemoveAsync($"deployment:{deploymentId}");

                    _logger.LogInformation("Deployment {DeploymentId} deleted", deploymentId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting deployment {DeploymentId}", deploymentId);
                return false;
            }
        }

        public async Task<PortfolioDeploymentResponse?> UpdateDeploymentDomainAsync(Guid deploymentId, string customDomain)
        {
            try
            {
                if (_deployments.TryGetValue(deploymentId, out var deployment) && deployment.VercelInfo != null)
                {
                    var success = await _vercelDeploymentService.SetCustomDomainAsync(
                        deployment.VercelInfo.VercelProjectId, 
                        customDomain);

                    if (success)
                    {
                        deployment.CustomDomain = customDomain;
                        deployment.VercelInfo.VercelAlias = customDomain;

                        // Update cache
                        await _cacheService.SetAsync($"deployment:{deploymentId}", deployment, TimeSpan.FromHours(24));

                        _logger.LogInformation("Custom domain {Domain} set for deployment {DeploymentId}", 
                            customDomain, deploymentId);
                        return deployment;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating domain for deployment {DeploymentId}", deploymentId);
                return null;
            }
        }

        #region Private Helper Methods

        private async Task<object> GetUserPortfolioDataAsync(Guid userId, Guid? portfolioId)
        {
            try
            {
                PortfolioDetailResponse? portfolio = null;

                if (portfolioId.HasValue)
                {
                    // Get specific portfolio
                    portfolio = await _portfolioQueryService.GetPortfolioByIdAsync(portfolioId.Value);
                }
                else
                {
                    // Get user's first published portfolio
                    var userPortfolios = await _portfolioQueryService.GetPortfoliosByUserIdAsync(userId);
                    var publishedPortfolio = userPortfolios?.FirstOrDefault(p => p.IsPublished);
                    
                    if (publishedPortfolio != null)
                    {
                        portfolio = await _portfolioQueryService.GetPortfolioByIdAsync(publishedPortfolio.Id);
                    }
                }

                if (portfolio == null)
                {
                    // Return default portfolio data
                    return new
                    {
                        name = "John Doe",
                        title = "Software Developer",
                        bio = "Passionate developer creating amazing web experiences.",
                        location = "San Francisco, CA",
                        avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=default",
                        projects = new object[] { },
                        experience = new object[] { },
                        skills = new object[] { },
                        blogPosts = new object[] { }
                    };
                }

                // Convert portfolio to deployment format
                return new
                {
                    name = portfolio.Title,
                    title = portfolio.Bio ?? "Professional",
                    bio = portfolio.Bio ?? "Welcome to my portfolio",
                    location = "Remote",
                    avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=" + portfolio.UserId,
                    projects = portfolio.Projects?.Select(p => new
                    {
                        title = p.Title,
                        description = p.Description,
                        technologies = p.Technologies,
                        link = p.DemoUrl
                    }).ToArray() ?? new object[] { },
                    experience = portfolio.Experience?.Select(e => new
                    {
                        position = e.JobTitle,
                        company = e.CompanyName,
                        description = e.Description,
                        startDate = e.StartDate.ToString("MMM yyyy"),
                        endDate = e.EndDate?.ToString("MMM yyyy")
                    }).ToArray() ?? new object[] { },
                    skills = portfolio.Skills?.Select(s => new
                    {
                        name = s.Name,
                        level = s.ProficiencyLevel?.ToString() ?? "Beginner"
                    }).ToArray() ?? new object[] { },
                    blogPosts = portfolio.BlogPosts?.Select(b => new
                    {
                        title = b.Title,
                        summary = b.Excerpt,
                        publishedAt = b.PublishedAt?.ToString("MMM dd, yyyy") ?? "",
                        link = "" // No direct link property available
                    }).ToArray() ?? new object[] { }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio data for user {UserId}", userId);
                throw;
            }
        }

        private string GenerateProjectName(Guid userId, string templateName)
        {
            // Vercel project names must be lowercase alphanumeric with hyphens
            // Cannot start or end with hyphen, max 100 characters
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var userPrefix = userId.ToString()[..8].ToLowerInvariant();
            var cleanTemplate = templateName.Replace("-", "").Replace("_", "").ToLowerInvariant();
            
            // Ensure it's alphanumeric and within limits
            var projectName = $"portfolio-{cleanTemplate}-{userPrefix}-{timestamp}";
            
            // Ensure it doesn't exceed 100 characters and is valid
            if (projectName.Length > 100)
            {
                projectName = $"portfolio-{cleanTemplate}-{timestamp}";
            }
            
            return projectName;
        }

        #endregion
    }
}
