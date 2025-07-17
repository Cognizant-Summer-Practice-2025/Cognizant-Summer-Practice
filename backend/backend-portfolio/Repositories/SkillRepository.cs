using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO;
using Microsoft.EntityFrameworkCore;

namespace backend_portfolio.Repositories
{
    public class SkillRepository : ISkillRepository
    {
        private readonly PortfolioDbContext _context;

        public SkillRepository(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            return await _context.Skills
                .Include(s => s.Portfolio)
                .ToListAsync();
        }

        public async Task<Skill?> GetSkillByIdAsync(Guid id)
        {
            return await _context.Skills
                .Include(s => s.Portfolio)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Skill>> GetSkillsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _context.Skills
                .Where(s => s.PortfolioId == portfolioId)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Skill> CreateSkillAsync(SkillRequestDto request)
        {
            var skill = new Skill
            {
                Id = Guid.NewGuid(),
                PortfolioId = request.PortfolioId,
                Name = request.Name,
                Category = request.Category,
                ProficiencyLevel = request.ProficiencyLevel,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<Skill?> UpdateSkillAsync(Guid id, SkillUpdateDto request)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return null;

            if (request.Name != null) skill.Name = request.Name;
            if (request.Category != null) skill.Category = request.Category;
            if (request.ProficiencyLevel.HasValue) skill.ProficiencyLevel = request.ProficiencyLevel.Value;
            if (request.DisplayOrder.HasValue) skill.DisplayOrder = request.DisplayOrder.Value;
            skill.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<bool> DeleteSkillAsync(Guid id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Skill>> GetSkillsByCategoryAsync(Guid portfolioId, string category)
        {
            return await _context.Skills
                .Where(s => s.PortfolioId == portfolioId && s.Category == category)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }
    }
}
