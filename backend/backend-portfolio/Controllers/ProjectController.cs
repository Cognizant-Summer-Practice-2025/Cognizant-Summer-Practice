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
using backend_portfolio.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectQueryService _projectQueryService;
        private readonly IProjectCommandService _projectCommandService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectQueryService projectQueryService,
            IProjectCommandService projectCommandService,
            ILogger<ProjectController> logger)
        {
            _projectQueryService = projectQueryService;
            _projectCommandService = projectCommandService;
            _logger = logger;
        }

        /// <summary>
        /// Get all projects
        /// </summary>
        /// <returns>List of all projects</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await _projectQueryService.GetAllProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all projects");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a project by ID
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            try
            {
                var project = await _projectQueryService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new { message = $"Project with ID {id} not found." });
                }
                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting project by ID: {ProjectId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all projects for a specific portfolio
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>List of projects for the portfolio</returns>
        [HttpGet("portfolio/{portfolioId}")]
        public async Task<IActionResult> GetProjectsByPortfolioId(Guid portfolioId)
        {
            try
            {
                var projects = await _projectQueryService.GetProjectsByPortfolioIdAsync(portfolioId);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting projects for portfolio: {PortfolioId}", portfolioId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all featured projects
        /// </summary>
        /// <returns>List of featured projects</returns>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedProjects()
        {
            try
            {
                var projects = await _projectQueryService.GetFeaturedProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured projects");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get featured projects for a specific portfolio
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>List of featured projects for the portfolio</returns>
        [HttpGet("portfolio/{portfolioId}/featured")]
        public async Task<IActionResult> GetFeaturedProjectsByPortfolioId(Guid portfolioId)
        {
            try
            {
                var projects = await _projectQueryService.GetFeaturedProjectsByPortfolioIdAsync(portfolioId);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured projects for portfolio: {PortfolioId}", portfolioId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="request">Project creation request</param>
        /// <returns>Created project</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateRequest request)
        {
            try
            {
                var project = await _projectCommandService.CreateProjectAsync(request);
                return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating project");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating project");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Project update request</param>
        /// <returns>Updated project</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdateRequest request)
        {
            try
            {
                var project = await _projectCommandService.UpdateProjectAsync(id, request);
                if (project == null)
                {
                    return NotFound(new { message = $"Project with ID {id} not found." });
                }
                return Ok(project);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating project: {ProjectId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating project: {ProjectId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            try
            {
                var result = await _projectCommandService.DeleteProjectAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Project with ID {id} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting project: {ProjectId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
} 