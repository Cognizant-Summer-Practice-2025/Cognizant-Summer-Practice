using backend_portfolio.DTO;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.ImageUpload.Response;
using backend_portfolio.Repositories;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;

namespace backend_portfolio.Services
{
    public class PortfolioCommandService : IPortfolioCommandService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IExperienceRepository _experienceRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IValidationService<PortfolioCreateRequest> _portfolioValidator;
        private readonly IValidationService<PortfolioUpdateRequest> _portfolioUpdateValidator;
        private readonly IPortfolioMapper _portfolioMapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PortfolioCommandService> _logger;

        public PortfolioCommandService(
            IPortfolioRepository portfolioRepository,
            IProjectRepository projectRepository,
            IExperienceRepository experienceRepository,
            ISkillRepository skillRepository,
            IBlogPostRepository blogPostRepository,
            IValidationService<PortfolioCreateRequest> portfolioValidator,
            IValidationService<PortfolioUpdateRequest> portfolioUpdateValidator,
            IPortfolioMapper portfolioMapper,
            ICacheService cacheService,
            ILogger<PortfolioCommandService> logger)
        {
            _portfolioRepository = portfolioRepository;
            _projectRepository = projectRepository;
            _experienceRepository = experienceRepository;
            _skillRepository = skillRepository;
            _blogPostRepository = blogPostRepository;
            _portfolioValidator = portfolioValidator;
            _portfolioUpdateValidator = portfolioUpdateValidator;
            _portfolioMapper = portfolioMapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Invalidates all portfolio-related cache entries when portfolios are published or updated
        /// </summary>
        private async Task InvalidatePortfolioCacheAsync()
        {
            try
            {
                // Clear all paginated portfolio cache entries (used by home page)
                await _cacheService.RemoveByPatternAsync("portfolios_paginated:.*");
                
                // Clear any other portfolio-related cache entries
                await _cacheService.RemoveByPatternAsync("portfolios:.*");
                
                _logger.LogInformation("Portfolio cache invalidated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate portfolio cache");
            }
        }

        public async Task<PortfolioResponse> CreatePortfolioAsync(PortfolioCreateRequest request)
        {
            try
            {
                var validationResult = await _portfolioValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");

                var portfolio = await _portfolioRepository.CreatePortfolioAsync(request);
                
                // Invalidate cache if portfolio is created as published
                if (portfolio.IsPublished)
                {
                    await InvalidatePortfolioCacheAsync();
                }
                
                return _portfolioMapper.MapToResponseDto(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating portfolio for user: {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<PortfolioResponse> CreatePortfolioAndGetIdAsync(PortfolioCreateRequest request)
        {
            try
            {
                var validationResult = await _portfolioValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");

                var portfolio = await _portfolioRepository.CreatePortfolioAsync(request);
                
                // Invalidate cache if portfolio is created as published
                if (portfolio.IsPublished)
                {
                    await InvalidatePortfolioCacheAsync();
                }
                
                return _portfolioMapper.MapToResponseDto(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating portfolio and getting ID for user: {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<BulkPortfolioContentResponse> SavePortfolioContentAsync(Guid portfolioId, BulkPortfolioContentRequest request)
        {
            try
            {
                request.PortfolioId = portfolioId;

                int projectsCreated = 0;
                int experienceCreated = 0;
                int skillsCreated = 0;
                int blogPostsCreated = 0;

                if (request.Projects?.Any() == true)
                {
                    foreach (var projectDto in request.Projects)
                    {
                        await _projectRepository.CreateProjectAsync(projectDto);
                        projectsCreated++;
                    }
                }

                if (request.Experience?.Any() == true)
                {
                    foreach (var experienceDto in request.Experience)
                    {
                        await _experienceRepository.CreateExperienceAsync(experienceDto);
                        experienceCreated++;
                    }
                }

                if (request.Skills?.Any() == true)
                {
                    foreach (var skillDto in request.Skills)
                    {
                        await _skillRepository.CreateSkillAsync(skillDto);
                        skillsCreated++;
                    }
                }

                if (request.BlogPosts?.Any() == true)
                {
                    foreach (var blogPostDto in request.BlogPosts)
                    {
                        await _blogPostRepository.CreateBlogPostAsync(blogPostDto);
                        blogPostsCreated++;
                    }
                }
 
                bool portfolioPublished = false;
                if (request.PublishPortfolio)
                {
                    var updateDto = new PortfolioUpdateRequest { IsPublished = true };
                    var updatedPortfolio = await _portfolioRepository.UpdatePortfolioAsync(portfolioId, updateDto);
                    portfolioPublished = updatedPortfolio != null;
                    
                    // Invalidate cache when portfolio is published
                    if (portfolioPublished)
                    {
                        await InvalidatePortfolioCacheAsync();
                    }
                }

                return new BulkPortfolioContentResponse
                {
                    Message = "Portfolio content saved successfully",
                    ProjectsCreated = projectsCreated,
                    ExperienceCreated = experienceCreated,
                    SkillsCreated = skillsCreated,
                    BlogPostsCreated = blogPostsCreated,
                    PortfolioPublished = portfolioPublished
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving portfolio content for portfolio: {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<PortfolioResponse?> UpdatePortfolioAsync(Guid id, PortfolioUpdateRequest request)
        {
            try
            {
                var validationResult = await _portfolioUpdateValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");

                var portfolio = await _portfolioRepository.UpdatePortfolioAsync(id, request);
                
                // Invalidate cache if portfolio was updated successfully and affects public visibility
                if (portfolio != null && ShouldInvalidateCache(request))
                {
                    await InvalidatePortfolioCacheAsync();
                }
                
                return portfolio != null ? _portfolioMapper.MapToResponseDto(portfolio) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating portfolio: {PortfolioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Determines if cache should be invalidated based on the update request
        /// </summary>
        private static bool ShouldInvalidateCache(PortfolioUpdateRequest request)
        {
            // Invalidate cache if:
            // - Portfolio is being published (IsPublished = true)
            // - Portfolio is being unpublished (IsPublished = false) 
            // - Visibility is being changed
            // - Title or Bio is being changed (affects home page display)
            return request.IsPublished.HasValue || 
                   request.Visibility.HasValue || 
                   !string.IsNullOrEmpty(request.Title) || 
                   !string.IsNullOrEmpty(request.Bio);
        }

        public async Task<bool> DeletePortfolioAsync(Guid id)
        {
            try
            {
                var result = await _portfolioRepository.DeletePortfolioAsync(id);
                
                // Invalidate cache when portfolio is deleted
                if (result)
                {
                    await InvalidatePortfolioCacheAsync();
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting portfolio: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<bool> IncrementViewCountAsync(Guid id)
        {
            try
            {
                return await _portfolioRepository.IncrementViewCountAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while incrementing view count for portfolio: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<bool> IncrementLikeCountAsync(Guid id)
        {
            try
            {
                return await _portfolioRepository.IncrementLikeCountAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while incrementing like count for portfolio: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<bool> DecrementLikeCountAsync(Guid id)
        {
            try
            {
                return await _portfolioRepository.DecrementLikeCountAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while decrementing like count for portfolio: {PortfolioId}", id);
                throw;
            }
        }

        public async Task DeleteAllUserPortfolioDataAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Starting cascade deletion of all portfolio data for user {UserId}", userId);

                // Get all portfolios for the user
                var portfolios = await _portfolioRepository.GetPortfoliosByUserIdAsync(userId);
                var portfolioIds = portfolios.Select(p => p.Id).ToList();

                if (portfolioIds.Any())
                {
                    _logger.LogInformation("Found {Count} portfolios to delete for user {UserId}", portfolioIds.Count, userId);

                    // Delete all related data for each portfolio
                    foreach (var portfolioId in portfolioIds)
                    {
                        // Delete projects
                        var projects = await _projectRepository.GetProjectsByPortfolioIdAsync(portfolioId);
                        foreach (var project in projects)
                        {
                            await _projectRepository.DeleteProjectAsync(project.Id);
                        }

                        // Delete experience
                        var experiences = await _experienceRepository.GetExperienceByPortfolioIdAsync(portfolioId);
                        foreach (var experience in experiences)
                        {
                            await _experienceRepository.DeleteExperienceAsync(experience.Id);
                        }

                        // Delete skills
                        var skills = await _skillRepository.GetSkillsByPortfolioIdAsync(portfolioId);
                        foreach (var skill in skills)
                        {
                            await _skillRepository.DeleteSkillAsync(skill.Id);
                        }

                        // Delete blog posts
                        var blogPosts = await _blogPostRepository.GetBlogPostsByPortfolioIdAsync(portfolioId);
                        foreach (var blogPost in blogPosts)
                        {
                            await _blogPostRepository.DeleteBlogPostAsync(blogPost.Id);
                        }

                        // Finally delete the portfolio itself
                        await _portfolioRepository.DeletePortfolioAsync(portfolioId);
                    }
                }

                // Invalidate cache
                await InvalidatePortfolioCacheAsync();

                _logger.LogInformation("Successfully deleted all portfolio data for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting all portfolio data for user {UserId}", userId);
                throw;
            }
        }
    }
} 