using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioTemplateController : ControllerBase
    {
        private readonly IPortfolioTemplateRepository _templateRepository;

        public PortfolioTemplateController(IPortfolioTemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _templateRepository.GetAllTemplatesAsync();
            var response = templates.Select(t => new PortfolioTemplateResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ComponentName = t.ComponentName,
                PreviewImageUrl = t.PreviewImageUrl,
                DefaultConfig = t.DefaultConfig,
                DefaultSections = t.DefaultSections,
                CustomizableOptions = t.CustomizableOptions,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            var template = await _templateRepository.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound($"Template with ID {id} not found.");
            }

            var response = new PortfolioTemplateResponseDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                ComponentName = template.ComponentName,
                PreviewImageUrl = template.PreviewImageUrl,
                DefaultConfig = template.DefaultConfig,
                DefaultSections = template.DefaultSections,
                CustomizableOptions = template.CustomizableOptions,
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
            return Ok(response);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTemplates()
        {
            var templates = await _templateRepository.GetActiveTemplatesAsync();
            var response = templates.Select(t => new PortfolioTemplateSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ComponentName = t.ComponentName,
                PreviewImageUrl = t.PreviewImageUrl,
                IsActive = t.IsActive
            });
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] PortfolioTemplateRequestDto request)
        {
            try
            {
                var template = await _templateRepository.CreateTemplateAsync(request);
                var response = new PortfolioTemplateResponseDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    ComponentName = template.ComponentName,
                    PreviewImageUrl = template.PreviewImageUrl,
                    DefaultConfig = template.DefaultConfig,
                    DefaultSections = template.DefaultSections,
                    CustomizableOptions = template.CustomizableOptions,
                    IsActive = template.IsActive,
                    CreatedAt = template.CreatedAt,
                    UpdatedAt = template.UpdatedAt
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] PortfolioTemplateUpdateDto request)
        {
            try
            {
                var template = await _templateRepository.UpdateTemplateAsync(id, request);
                if (template == null)
                    return NotFound($"Template with ID {id} not found.");

                var response = new PortfolioTemplateResponseDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    ComponentName = template.ComponentName,
                    PreviewImageUrl = template.PreviewImageUrl,
                    DefaultConfig = template.DefaultConfig,
                    DefaultSections = template.DefaultSections,
                    CustomizableOptions = template.CustomizableOptions,
                    IsActive = template.IsActive,
                    CreatedAt = template.CreatedAt,
                    UpdatedAt = template.UpdatedAt
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var result = await _templateRepository.DeleteTemplateAsync(id);
            if (!result)
                return NotFound($"Template with ID {id} not found.");
            return NoContent();
        }
    }
}
