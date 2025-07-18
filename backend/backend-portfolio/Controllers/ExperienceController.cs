using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExperienceController : ControllerBase
    {
        private readonly IExperienceRepository _experienceRepository;

        public ExperienceController(IExperienceRepository experienceRepository)
        {
            _experienceRepository = experienceRepository;
        }

        [HttpGet("portfolio/{portfolioId}")]
        public async Task<IActionResult> GetExperienceByPortfolioId(Guid portfolioId)
        {
            try
            {
                var experiences = await _experienceRepository.GetExperienceByPortfolioIdAsync(portfolioId);
                var response = experiences.Select(e => new ExperienceResponseDto
                {
                    Id = e.Id,
                    PortfolioId = e.PortfolioId,
                    JobTitle = e.JobTitle,
                    CompanyName = e.CompanyName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Description = e.Description,
                    SkillsUsed = e.SkillsUsed,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExperienceById(Guid id)
        {
            try
            {
                var experience = await _experienceRepository.GetExperienceByIdAsync(id);
                if (experience == null)
                {
                    return NotFound($"Experience with ID {id} not found.");
                }

                var response = new ExperienceResponseDto
                {
                    Id = experience.Id,
                    PortfolioId = experience.PortfolioId,
                    JobTitle = experience.JobTitle,
                    CompanyName = experience.CompanyName,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    IsCurrent = experience.IsCurrent,
                    Description = experience.Description,
                    SkillsUsed = experience.SkillsUsed,
                    CreatedAt = experience.CreatedAt,
                    UpdatedAt = experience.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateExperience([FromBody] ExperienceRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.JobTitle))
                {
                    return BadRequest("Job title is required.");
                }

                if (string.IsNullOrWhiteSpace(request.CompanyName))
                {
                    return BadRequest("Company name is required.");
                }

                var experience = await _experienceRepository.CreateExperienceAsync(request);
                var response = new ExperienceResponseDto
                {
                    Id = experience.Id,
                    PortfolioId = experience.PortfolioId,
                    JobTitle = experience.JobTitle,
                    CompanyName = experience.CompanyName,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    IsCurrent = experience.IsCurrent,
                    Description = experience.Description,
                    SkillsUsed = experience.SkillsUsed,
                    CreatedAt = experience.CreatedAt,
                    UpdatedAt = experience.UpdatedAt
                };

                return CreatedAtAction(nameof(GetExperienceById), new { id = experience.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating experience: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExperience(Guid id, [FromBody] ExperienceUpdateDto request)
        {
            try
            {
                var experience = await _experienceRepository.UpdateExperienceAsync(id, request);
                if (experience == null)
                {
                    return NotFound($"Experience with ID {id} not found.");
                }

                var response = new ExperienceResponseDto
                {
                    Id = experience.Id,
                    PortfolioId = experience.PortfolioId,
                    JobTitle = experience.JobTitle,
                    CompanyName = experience.CompanyName,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    IsCurrent = experience.IsCurrent,
                    Description = experience.Description,
                    SkillsUsed = experience.SkillsUsed,
                    CreatedAt = experience.CreatedAt,
                    UpdatedAt = experience.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating experience: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExperience(Guid id)
        {
            try
            {
                var result = await _experienceRepository.DeleteExperienceAsync(id);
                if (!result)
                {
                    return NotFound($"Experience with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExperience()
        {
            try
            {
                var experiences = await _experienceRepository.GetAllExperienceAsync();
                var response = experiences.Select(e => new ExperienceResponseDto
                {
                    Id = e.Id,
                    PortfolioId = e.PortfolioId,
                    JobTitle = e.JobTitle,
                    CompanyName = e.CompanyName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Description = e.Description,
                    SkillsUsed = e.SkillsUsed,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
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