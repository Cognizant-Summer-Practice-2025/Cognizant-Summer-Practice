using backend_portfolio.Data;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.Models;
using backend_portfolio.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace backend_portfolio.Services
{
    public class PortfolioTemplateService : IPortfolioTemplateService
    {
        private readonly PortfolioDbContext _context;
        private readonly ILogger<PortfolioTemplateService> _logger;

        public PortfolioTemplateService(PortfolioDbContext context, ILogger<PortfolioTemplateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PortfolioTemplateResponse>> GetAllTemplatesAsync()
        {
            var templates = await _context.PortfolioTemplates
                .OrderBy(t => t.Name)
                .ToListAsync();

            return templates.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<PortfolioTemplateResponse>> GetActiveTemplatesAsync()
        {
            var templates = await _context.PortfolioTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return templates.Select(MapToResponseDto);
        }

        public async Task<PortfolioTemplateResponse?> GetTemplateByIdAsync(Guid id)
        {
            var template = await _context.PortfolioTemplates
                .FirstOrDefaultAsync(t => t.Id == id);

            return template != null ? MapToResponseDto(template) : null;
        }

        public async Task<PortfolioTemplateResponse?> GetTemplateByNameAsync(string name)
        {
            var template = await _context.PortfolioTemplates
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

            return template != null ? MapToResponseDto(template) : null;
        }

        public async Task<PortfolioTemplateResponse> CreateTemplateAsync(PortfolioTemplateCreateRequest request)
        {
            var template = new PortfolioTemplate
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                PreviewImageUrl = request.PreviewImageUrl,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PortfolioTemplates.Add(template);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new portfolio template: {TemplateName} with ID: {TemplateId}", 
                template.Name, template.Id);

            return MapToResponseDto(template);
        }

        public async Task<PortfolioTemplateResponse?> UpdateTemplateAsync(Guid id, PortfolioTemplateUpdateRequest request)
        {
            var template = await _context.PortfolioTemplates
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return null;

            if (request.Name != null)
                template.Name = request.Name;
            if (request.Description != null)
                template.Description = request.Description;
            if (request.PreviewImageUrl != null)
                template.PreviewImageUrl = request.PreviewImageUrl;
            if (request.IsActive.HasValue)
                template.IsActive = request.IsActive.Value;

            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated portfolio template: {TemplateName} with ID: {TemplateId}", 
                template.Name, template.Id);

            return MapToResponseDto(template);
        }

        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            var template = await _context.PortfolioTemplates
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return false;

            // Check if any portfolios are using this template
            var portfoliosUsingTemplate = await _context.Portfolios
                .AnyAsync(p => p.TemplateId == id);

            if (portfoliosUsingTemplate)
            {
                // Instead of deleting, mark as inactive
                template.IsActive = false;
                template.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Marked portfolio template as inactive: {TemplateName} with ID: {TemplateId}", 
                    template.Name, template.Id);
            }
            else
            {
                // Safe to delete if no portfolios are using it
                _context.PortfolioTemplates.Remove(template);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Deleted portfolio template: {TemplateName} with ID: {TemplateId}", 
                    template.Name, template.Id);
            }

            return true;
        }

        public async Task SeedDefaultTemplatesAsync()
        {
            var defaultTemplates = new[]
            {
                new { Name = "Gabriel Bârzu", Description = "Modern minimalist design with clean typography and structured layout", PreviewImageUrl = "/templates/gabriel-barzu/preview.jpg" },
                new { Name = "Modern", Description = "Contemporary design with glassmorphism effects, dark mode support, and smooth animations", PreviewImageUrl = "/templates/modern/preview.jpg" },
                new { Name = "Creative", Description = "Bold and artistic layout with vibrant colors and creative elements", PreviewImageUrl = "/templates/creative/preview.jpg" },
                new { Name = "Professional", Description = "Clean corporate design perfect for business professionals", PreviewImageUrl = "/templates/professional/preview.jpg" }
            };

            foreach (var templateData in defaultTemplates)
            {
                var existingTemplate = await _context.PortfolioTemplates
                    .FirstOrDefaultAsync(t => t.Name == templateData.Name);

                if (existingTemplate == null)
                {
                    var template = new PortfolioTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = templateData.Name,
                        Description = templateData.Description,
                        PreviewImageUrl = templateData.PreviewImageUrl,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.PortfolioTemplates.Add(template);
                    _logger.LogInformation("Seeded template: {TemplateName} with ID: {TemplateId}", 
                        template.Name, template.Id);
                }
            }

            await _context.SaveChangesAsync();
        }

        private static PortfolioTemplateResponse MapToResponseDto(PortfolioTemplate template)
        {
            return new PortfolioTemplateResponse
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                PreviewImageUrl = template.PreviewImageUrl,
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }
    }
} 