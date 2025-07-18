using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ISkillRepository _skillRepository;

        public SkillController(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        [HttpGet("portfolio/{portfolioId}")]
        public async Task<IActionResult> GetSkillsByPortfolioId(Guid portfolioId)
        {
            try
            {
                var skills = await _skillRepository.GetSkillsByPortfolioIdAsync(portfolioId);
                var response = skills.Select(s => new SkillResponseDto
                {
                    Id = s.Id,
                    PortfolioId = s.PortfolioId,
                    Name = s.Name,
                    CategoryType = s.CategoryType,
                    Subcategory = s.Subcategory,
                    Category = s.Category,
                    ProficiencyLevel = s.ProficiencyLevel,
                    DisplayOrder = s.DisplayOrder,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSkillById(Guid id)
        {
            try
            {
                var skill = await _skillRepository.GetSkillByIdAsync(id);
                if (skill == null)
                {
                    return NotFound($"Skill with ID {id} not found.");
                }

                var response = new SkillResponseDto
                {
                    Id = skill.Id,
                    PortfolioId = skill.PortfolioId,
                    Name = skill.Name,
                    CategoryType = skill.CategoryType,
                    Subcategory = skill.Subcategory,
                    Category = skill.Category,
                    ProficiencyLevel = skill.ProficiencyLevel,
                    DisplayOrder = skill.DisplayOrder,
                    CreatedAt = skill.CreatedAt,
                    UpdatedAt = skill.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSkill([FromBody] SkillRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("Skill name is required.");
                }

                var skill = await _skillRepository.CreateSkillAsync(request);
                var response = new SkillResponseDto
                {
                    Id = skill.Id,
                    PortfolioId = skill.PortfolioId,
                    Name = skill.Name,
                    CategoryType = skill.CategoryType,
                    Subcategory = skill.Subcategory,
                    Category = skill.Category,
                    ProficiencyLevel = skill.ProficiencyLevel,
                    DisplayOrder = skill.DisplayOrder,
                    CreatedAt = skill.CreatedAt,
                    UpdatedAt = skill.UpdatedAt
                };

                return CreatedAtAction(nameof(GetSkillById), new { id = skill.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating skill: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] SkillUpdateDto request)
        {
            try
            {
                var skill = await _skillRepository.UpdateSkillAsync(id, request);
                if (skill == null)
                {
                    return NotFound($"Skill with ID {id} not found.");
                }

                var response = new SkillResponseDto
                {
                    Id = skill.Id,
                    PortfolioId = skill.PortfolioId,
                    Name = skill.Name,
                    CategoryType = skill.CategoryType,
                    Subcategory = skill.Subcategory,
                    Category = skill.Category,
                    ProficiencyLevel = skill.ProficiencyLevel,
                    DisplayOrder = skill.DisplayOrder,
                    CreatedAt = skill.CreatedAt,
                    UpdatedAt = skill.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating skill: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(Guid id)
        {
            try
            {
                var result = await _skillRepository.DeleteSkillAsync(id);
                if (!result)
                {
                    return NotFound($"Skill with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSkills()
        {
            try
            {
                var skills = await _skillRepository.GetAllSkillsAsync();
                var response = skills.Select(s => new SkillResponseDto
                {
                    Id = s.Id,
                    PortfolioId = s.PortfolioId,
                    Name = s.Name,
                    CategoryType = s.CategoryType,
                    Subcategory = s.Subcategory,
                    Category = s.Category,
                    ProficiencyLevel = s.ProficiencyLevel,
                    DisplayOrder = s.DisplayOrder,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
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