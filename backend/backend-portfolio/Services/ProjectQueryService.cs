using backend_portfolio.DTO;
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
    public class ProjectQueryService : IProjectQueryService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMapper _projectMapper;
        private readonly ILogger<ProjectQueryService> _logger;

        public ProjectQueryService(
            IProjectRepository projectRepository,
            IProjectMapper projectMapper,
            ILogger<ProjectQueryService> logger)
        {
            _projectRepository = projectRepository;
            _projectMapper = projectMapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync()
        {
            try
            {
                var projects = await _projectRepository.GetAllProjectsAsync();
                return _projectMapper.MapToResponseDtos(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all projects");
                throw;
            }
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id)
        {
            try
            {
                var project = await _projectRepository.GetProjectByIdAsync(id);
                return project != null ? _projectMapper.MapToResponseDto(project) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting project by ID: {ProjectId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ProjectResponse>> GetProjectsByPortfolioIdAsync(Guid portfolioId)
        {
            try
            {
                var projects = await _projectRepository.GetProjectsByPortfolioIdAsync(portfolioId);
                return _projectMapper.MapToResponseDtos(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting projects for portfolio: {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<IEnumerable<ProjectResponse>> GetFeaturedProjectsAsync()
        {
            try
            {
                var projects = await _projectRepository.GetFeaturedProjectsAsync();
                return _projectMapper.MapToResponseDtos(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured projects");
                throw;
            }
        }

        public async Task<IEnumerable<ProjectResponse>> GetFeaturedProjectsByPortfolioIdAsync(Guid portfolioId)
        {
            try
            {
                var projects = await _projectRepository.GetFeaturedProjectsByPortfolioIdAsync(portfolioId);
                return _projectMapper.MapToResponseDtos(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured projects for portfolio: {PortfolioId}", portfolioId);
                throw;
            }
        }
    }
} 