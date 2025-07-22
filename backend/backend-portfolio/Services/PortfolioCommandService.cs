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
        private readonly PortfolioMapper _portfolioMapper;
        private readonly ILogger<PortfolioCommandService> _logger;

        public PortfolioCommandService(
            IPortfolioRepository portfolioRepository,
            IProjectRepository projectRepository,
            IExperienceRepository experienceRepository,
            ISkillRepository skillRepository,
            IBlogPostRepository blogPostRepository,
            IValidationService<PortfolioCreateRequest> portfolioValidator,
            IValidationService<PortfolioUpdateRequest> portfolioUpdateValidator,
            PortfolioMapper portfolioMapper,
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
            _logger = logger;
        }

        public async Task<PortfolioResponse> CreatePortfolioAsync(PortfolioCreateRequest request)
        {
            try
            {
                var validationResult = await _portfolioValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");

                var portfolio = await _portfolioRepository.CreatePortfolioAsync(request);
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
                return portfolio != null ? _portfolioMapper.MapToResponseDto(portfolio) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating portfolio: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<bool> DeletePortfolioAsync(Guid id)
        {
            try
            {
                return await _portfolioRepository.DeletePortfolioAsync(id);
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
    }
} 