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
    public class ProjectCommandService : IProjectCommandService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IValidationService<ProjectCreateRequest> _projectValidator;
        private readonly IValidationService<ProjectUpdateRequest> _projectUpdateValidator;
        private readonly IProjectMapper _projectMapper;
        private readonly ILogger<ProjectCommandService> _logger;

        public ProjectCommandService(
            IProjectRepository projectRepository,
            IValidationService<ProjectCreateRequest> projectValidator,
            IValidationService<ProjectUpdateRequest> projectUpdateValidator,
            IProjectMapper projectMapper,
            ILogger<ProjectCommandService> logger)
        {
            _projectRepository = projectRepository;
            _projectValidator = projectValidator;
            _projectUpdateValidator = projectUpdateValidator;
            _projectMapper = projectMapper;
            _logger = logger;
        }

        public async Task<ProjectResponse> CreateProjectAsync(ProjectCreateRequest request)
        {
            try
            {
                var validationResult = await _projectValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");

                var project = await _projectRepository.CreateProjectAsync(request);
                return _projectMapper.MapToResponseDto(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating project for portfolio: {PortfolioId}", request.PortfolioId);
                throw;
            }
        }

        public async Task<ProjectResponse?> UpdateProjectAsync(Guid id, ProjectUpdateRequest request)
        {
            try
            {
                var validationResult = await _projectUpdateValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");

                var project = await _projectRepository.UpdateProjectAsync(id, request);
                return project != null ? _projectMapper.MapToResponseDto(project) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating project: {ProjectId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            try
            {
                // First check if the project exists
                var existingProject = await _projectRepository.GetProjectByIdAsync(id);
                if (existingProject == null)
                    return false;

                return await _projectRepository.DeleteProjectAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting project: {ProjectId}", id);
                throw;
            }
        }
    }
} 