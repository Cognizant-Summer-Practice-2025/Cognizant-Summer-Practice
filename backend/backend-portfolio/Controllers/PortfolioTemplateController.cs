using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioTemplateController : ControllerBase
    {
        private readonly IPortfolioTemplateService _portfolioTemplateService;
        private readonly ILogger<PortfolioTemplateController> _logger;

        public PortfolioTemplateController(
            IPortfolioTemplateService portfolioTemplateService,
            ILogger<PortfolioTemplateController> logger)
        {
            _portfolioTemplateService = portfolioTemplateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTemplates()
        {
            try
            {
                var templates = await _portfolioTemplateService.GetAllTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all templates");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTemplates()
        {
            try
            {
                var templates = await _portfolioTemplateService.GetActiveTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active templates");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Template ID cannot be empty.");

                var template = await _portfolioTemplateService.GetTemplateByIdAsync(id);
                if (template == null)
                    return NotFound($"Template with ID {id} not found.");

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting template: {TemplateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetTemplateByName(string name)
        {
            try
            {
                var template = await _portfolioTemplateService.GetTemplateByNameAsync(name);
                if (template == null)
                    return NotFound($"Template with name '{name}' not found.");

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting template by name: {TemplateName}", name);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] PortfolioTemplateCreateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request cannot be null.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var template = await _portfolioTemplateService.CreateTemplateAsync(request);
                return CreatedAtAction(nameof(GetTemplateById), new { id = template.Id }, template);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating template");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating template");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] PortfolioTemplateUpdateRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Template ID cannot be empty.");

                if (request == null)
                    return BadRequest("Request cannot be null.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var template = await _portfolioTemplateService.UpdateTemplateAsync(id, request);
                if (template == null)
                    return NotFound($"Template with ID {id} not found.");

                return Ok(template);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating template: {TemplateId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating template: {TemplateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Template ID cannot be empty.");

                var result = await _portfolioTemplateService.DeleteTemplateAsync(id);
                if (!result)
                    return NotFound($"Template with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting template: {TemplateId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedDefaultTemplates()
        {
            try
            {
                await _portfolioTemplateService.SeedDefaultTemplatesAsync();
                return Ok(new { message = "Default templates seeded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding default templates");
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 