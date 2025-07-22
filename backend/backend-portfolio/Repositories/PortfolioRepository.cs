using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;
using Microsoft.EntityFrameworkCore;

namespace backend_portfolio.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly PortfolioDbContext _context;

        public PortfolioRepository(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<Portfolio>> GetAllPortfoliosAsync()
        {
            return await _context.Portfolios
                .Include(p => p.Template)
                .Include(p => p.Projects)
                .Include(p => p.Experience)
                .Include(p => p.Skills)
                .Include(p => p.BlogPosts)
                .ToListAsync();
        }

        public async Task<Portfolio?> GetPortfolioByIdAsync(Guid id)
        {
            return await _context.Portfolios
                .Include(p => p.Template)
                .Include(p => p.Projects)
                .Include(p => p.Experience)
                .Include(p => p.Skills)
                .Include(p => p.BlogPosts)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Portfolio>> GetPortfoliosByUserIdAsync(Guid userId)
        {
            return await _context.Portfolios
                .Include(p => p.Template)
                .Include(p => p.Projects)
                .Include(p => p.Experience)
                .Include(p => p.Skills)
                .Include(p => p.BlogPosts)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Portfolio> CreatePortfolioAsync(PortfolioCreateRequest request)
        {
            // Find template by name
            var template = await _context.PortfolioTemplates
                .FirstOrDefaultAsync(t => t.Name == request.TemplateName && t.IsActive);
            
            if (template == null)
            {
                throw new ArgumentException($"Template with name '{request.TemplateName}' not found or is inactive.");
            }

            var portfolio = new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TemplateId = template.Id, // Use the found template's ID
                Title = request.Title,
                Bio = request.Bio,
                Visibility = request.Visibility,
                IsPublished = request.IsPublished,
                Components = request.Components,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();
            return portfolio;
        }

        public async Task<Portfolio?> UpdatePortfolioAsync(Guid id, PortfolioUpdateRequest request)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null) return null;

            // Handle template name update
            if (!string.IsNullOrEmpty(request.TemplateName))
            {
                var template = await _context.PortfolioTemplates
                    .FirstOrDefaultAsync(t => t.Name == request.TemplateName && t.IsActive);
                
                if (template == null)
                {
                    throw new ArgumentException($"Template with name '{request.TemplateName}' not found or is inactive.");
                }
                
                portfolio.TemplateId = template.Id;
            }

            if (request.Title != null) portfolio.Title = request.Title;
            if (request.Bio != null) portfolio.Bio = request.Bio;
            if (request.Visibility.HasValue) portfolio.Visibility = (Visibility)request.Visibility.Value;
            if (request.IsPublished.HasValue) portfolio.IsPublished = request.IsPublished.Value;
            if (request.Components != null) portfolio.Components = request.Components;
            portfolio.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return portfolio;
        }

        public async Task<bool> DeletePortfolioAsync(Guid id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null) return false;

            _context.Portfolios.Remove(portfolio);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Portfolio>> GetPublishedPortfoliosAsync()
        {
            return await _context.Portfolios
                .Include(p => p.Template)
                .Where(p => p.IsPublished && p.Visibility == Visibility.Public)
                .ToListAsync();
        }

        public async Task<List<Portfolio>> GetPortfoliosByVisibilityAsync(Visibility visibility)
        {
            return await _context.Portfolios
                .Include(p => p.Template)
                .Where(p => p.Visibility == visibility)
                .ToListAsync();
        }

        public async Task<bool> IncrementViewCountAsync(Guid id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null) return false;

            portfolio.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementLikeCountAsync(Guid id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null) return false;

            portfolio.LikeCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DecrementLikeCountAsync(Guid id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null) return false;

            portfolio.LikeCount = Math.Max(0, portfolio.LikeCount - 1);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
