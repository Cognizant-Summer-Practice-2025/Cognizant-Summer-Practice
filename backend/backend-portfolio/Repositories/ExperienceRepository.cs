using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO;
using Microsoft.EntityFrameworkCore;

namespace backend_portfolio.Repositories
{
    public class ExperienceRepository : IExperienceRepository
    {
        private readonly PortfolioDbContext _context;

        public ExperienceRepository(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<Experience>> GetAllExperienceAsync()
        {
            return await _context.Experience
                .Include(e => e.Portfolio)
                .ToListAsync();
        }

        public async Task<Experience?> GetExperienceByIdAsync(Guid id)
        {
            return await _context.Experience
                .Include(e => e.Portfolio)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Experience>> GetExperienceByPortfolioIdAsync(Guid portfolioId)
        {
            return await _context.Experience
                .Where(e => e.PortfolioId == portfolioId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<Experience> CreateExperienceAsync(ExperienceRequestDto request)
        {
            var experience = new Experience
            {
                Id = Guid.NewGuid(),
                PortfolioId = request.PortfolioId,
                JobTitle = request.JobTitle,
                CompanyName = request.CompanyName,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsCurrent = request.IsCurrent,
                Description = request.Description,
                SkillsUsed = request.SkillsUsed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Experience.AddAsync(experience);
            await _context.SaveChangesAsync();
            return experience;
        }

        public async Task<Experience?> UpdateExperienceAsync(Guid id, ExperienceUpdateDto request)
        {
            var experience = await _context.Experience.FindAsync(id);
            if (experience == null) return null;

            if (request.JobTitle != null) experience.JobTitle = request.JobTitle;
            if (request.CompanyName != null) experience.CompanyName = request.CompanyName;
            if (request.StartDate.HasValue) experience.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) experience.EndDate = request.EndDate.Value;
            if (request.IsCurrent.HasValue) experience.IsCurrent = request.IsCurrent.Value;
            if (request.Description != null) experience.Description = request.Description;
            if (request.SkillsUsed != null) experience.SkillsUsed = request.SkillsUsed;
            experience.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return experience;
        }

        public async Task<bool> DeleteExperienceAsync(Guid id)
        {
            var experience = await _context.Experience.FindAsync(id);
            if (experience == null) return false;

            _context.Experience.Remove(experience);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Experience>> GetCurrentExperienceAsync(Guid portfolioId)
        {
            return await _context.Experience
                .Where(e => e.PortfolioId == portfolioId && e.IsCurrent)
                .ToListAsync();
        }
    }
}
