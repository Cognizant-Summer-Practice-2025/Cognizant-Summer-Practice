using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO;
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

        public async Task<Portfolio> CreatePortfolioAsync(PortfolioRequestDto request)
        {
            var portfolio = new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TemplateId = request.TemplateId,
                Title = request.Title,
                Bio = request.Bio,
                CustomConfig = request.CustomConfig,
                CustomSections = request.CustomSections,
                Visibility = request.Visibility,
                IsPublished = request.IsPublished,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();
            return portfolio;
        }

        public async Task<Portfolio?> UpdatePortfolioAsync(Guid id, PortfolioUpdateDto request)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null) return null;

            if (request.TemplateId.HasValue) portfolio.TemplateId = request.TemplateId.Value;
            if (request.Title != null) portfolio.Title = request.Title;
            if (request.Bio != null) portfolio.Bio = request.Bio;
            if (request.CustomConfig != null) portfolio.CustomConfig = request.CustomConfig;
            if (request.CustomSections != null) portfolio.CustomSections = request.CustomSections;
            if (request.Visibility.HasValue) portfolio.Visibility = request.Visibility.Value;
            if (request.IsPublished.HasValue) portfolio.IsPublished = request.IsPublished.Value;
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
