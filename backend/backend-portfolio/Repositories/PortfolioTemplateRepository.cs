using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Request;
using Microsoft.EntityFrameworkCore;

namespace backend_portfolio.Repositories
{
    public class PortfolioTemplateRepository : IPortfolioTemplateRepository
    {
        private readonly PortfolioDbContext _context;

        public PortfolioTemplateRepository(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<PortfolioTemplate>> GetAllTemplatesAsync()
        {
            return await _context.PortfolioTemplates.ToListAsync();
        }

        public async Task<PortfolioTemplate?> GetTemplateByIdAsync(Guid id)
        {
            return await _context.PortfolioTemplates.FindAsync(id);
        }

        public async Task<PortfolioTemplate> CreateTemplateAsync(PortfolioTemplateCreateRequest request)
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

            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<PortfolioTemplate?> UpdateTemplateAsync(Guid id, PortfolioTemplateUpdateRequest request)
        {
            var template = await _context.PortfolioTemplates.FindAsync(id);
            if (template == null) return null;

            if (request.Name != null) template.Name = request.Name;
            if (request.Description != null) template.Description = request.Description;
            if (request.PreviewImageUrl != null) template.PreviewImageUrl = request.PreviewImageUrl;
            if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            var template = await _context.PortfolioTemplates.FindAsync(id);
            if (template == null) return false;

            _context.PortfolioTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PortfolioTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.PortfolioTemplates
                .Where(t => t.IsActive)
                .ToListAsync();
        }
    }
}
