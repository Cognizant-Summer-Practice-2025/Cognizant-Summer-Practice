using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        [HttpGet("portfolio/{portfolioId}")]
        public async Task<IActionResult> GetProjectsByPortfolioId(Guid portfolioId)
        {
            try
            {
                var projects = await _projectRepository.GetProjectsByPortfolioIdAsync(portfolioId);
                var response = projects.Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    PortfolioId = p.PortfolioId,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    DemoUrl = p.DemoUrl,
                    GithubUrl = p.GithubUrl,
                    Technologies = p.Technologies,
                    Featured = p.Featured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            try
            {
                var project = await _projectRepository.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                var response = new ProjectResponseDto
                {
                    Id = project.Id,
                    PortfolioId = project.PortfolioId,
                    Title = project.Title,
                    Description = project.Description,
                    ImageUrl = project.ImageUrl,
                    DemoUrl = project.DemoUrl,
                    GithubUrl = project.GithubUrl,
                    Technologies = project.Technologies,
                    Featured = project.Featured,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest("Project title is required.");
                }

                var project = await _projectRepository.CreateProjectAsync(request);
                var response = new ProjectResponseDto
                {
                    Id = project.Id,
                    PortfolioId = project.PortfolioId,
                    Title = project.Title,
                    Description = project.Description,
                    ImageUrl = project.ImageUrl,
                    DemoUrl = project.DemoUrl,
                    GithubUrl = project.GithubUrl,
                    Technologies = project.Technologies,
                    Featured = project.Featured,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                };

                return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating project: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdateDto request)
        {
            try
            {
                var project = await _projectRepository.UpdateProjectAsync(id, request);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                var response = new ProjectResponseDto
                {
                    Id = project.Id,
                    PortfolioId = project.PortfolioId,
                    Title = project.Title,
                    Description = project.Description,
                    ImageUrl = project.ImageUrl,
                    DemoUrl = project.DemoUrl,
                    GithubUrl = project.GithubUrl,
                    Technologies = project.Technologies,
                    Featured = project.Featured,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating project: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            try
            {
                var result = await _projectRepository.DeleteProjectAsync(id);
                if (!result)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await _projectRepository.GetAllProjectsAsync();
                var response = projects.Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    PortfolioId = p.PortfolioId,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    DemoUrl = p.DemoUrl,
                    GithubUrl = p.GithubUrl,
                    Technologies = p.Technologies,
                    Featured = p.Featured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 