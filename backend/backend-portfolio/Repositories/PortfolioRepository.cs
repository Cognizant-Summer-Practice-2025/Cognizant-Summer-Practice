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
using System.Linq;

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
            var portfolios = await _context.Portfolios
                .Include(p => p.Template)
                .AsNoTracking()
                .ToListAsync();

            if (!portfolios.Any()) return portfolios;

            var portfolioIds = portfolios.Select(p => p.Id).ToList();

            var projects = await _context.Projects
                .Where(p => portfolioIds.Contains(p.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            var experiences = await _context.Experience
                .Where(e => portfolioIds.Contains(e.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            var skills = await _context.Skills
                .Where(s => portfolioIds.Contains(s.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            var blogPosts = await _context.BlogPosts
                .Where(b => portfolioIds.Contains(b.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            foreach (var portfolio in portfolios)
            {
                portfolio.Projects = projects.Where(p => p.PortfolioId == portfolio.Id).ToList();
                portfolio.Experience = experiences.Where(e => e.PortfolioId == portfolio.Id).ToList();
                portfolio.Skills = skills.Where(s => s.PortfolioId == portfolio.Id).ToList();
                portfolio.BlogPosts = blogPosts.Where(b => b.PortfolioId == portfolio.Id).ToList();
            }

            return portfolios;
        }

        public async Task<Portfolio?> GetPortfolioByIdAsync(Guid id)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Template)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (portfolio == null) return null;

            var projects = await _context.Projects
                .Where(p => p.PortfolioId == id)
                .AsNoTracking()
                .ToListAsync();

            var experiences = await _context.Experience
                .Where(e => e.PortfolioId == id)
                .AsNoTracking()
                .ToListAsync();

            var skills = await _context.Skills
                .Where(s => s.PortfolioId == id)
                .AsNoTracking()
                .ToListAsync();

            var blogPosts = await _context.BlogPosts
                .Where(b => b.PortfolioId == id)
                .AsNoTracking()
                .ToListAsync();

            portfolio.Projects = projects;
            portfolio.Experience = experiences;
            portfolio.Skills = skills;
            portfolio.BlogPosts = blogPosts;

            return portfolio;
        }

        public async Task<List<Portfolio>> GetPortfoliosByUserIdAsync(Guid userId)
        {
            var portfolios = await _context.Portfolios
                .Include(p => p.Template)
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            if (!portfolios.Any()) return portfolios;

            var portfolioIds = portfolios.Select(p => p.Id).ToList();

            var projects = await _context.Projects
                .Where(p => portfolioIds.Contains(p.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            var experiences = await _context.Experience
                .Where(e => portfolioIds.Contains(e.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            var skills = await _context.Skills
                .Where(s => portfolioIds.Contains(s.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            var blogPosts = await _context.BlogPosts
                .Where(b => portfolioIds.Contains(b.PortfolioId))
                .AsNoTracking()
                .ToListAsync();

            var projectsLookup = projects.ToLookup(p => p.PortfolioId);
            var experiencesLookup = experiences.ToLookup(e => e.PortfolioId);
            var skillsLookup = skills.ToLookup(s => s.PortfolioId);
            var blogPostsLookup = blogPosts.ToLookup(b => b.PortfolioId);

            foreach (var portfolio in portfolios)
            {
                portfolio.Projects = projectsLookup[portfolio.Id].ToList();
                portfolio.Experience = experiencesLookup[portfolio.Id].ToList();
                portfolio.Skills = skillsLookup[portfolio.Id].ToList();
                portfolio.BlogPosts = blogPostsLookup[portfolio.Id].ToList();
            }

            return portfolios;
        }

        public async Task<Portfolio> CreatePortfolioAsync(PortfolioCreateRequest request)
        {
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
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Portfolio>> GetPortfoliosByVisibilityAsync(Visibility visibility)
        {
            return await _context.Portfolios
                .Include(p => p.Template)
                .Where(p => p.Visibility == visibility)
                .AsNoTracking()
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
