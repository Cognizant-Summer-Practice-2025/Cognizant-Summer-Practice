using backend_portfolio.Models;
using backend_portfolio.DTO;

namespace backend_portfolio.Repositories
{
    public interface ISkillRepository
    {
        Task<List<Skill>> GetAllSkillsAsync();
        Task<Skill?> GetSkillByIdAsync(Guid id);
        Task<List<Skill>> GetSkillsByPortfolioIdAsync(Guid portfolioId);
        Task<Skill> CreateSkillAsync(SkillRequestDto request);
        Task<Skill?> UpdateSkillAsync(Guid id, SkillUpdateDto request);
        Task<bool> DeleteSkillAsync(Guid id);
        Task<List<Skill>> GetSkillsByCategoryAsync(Guid portfolioId, string category);
        Task<List<Skill>> GetSkillsByCategoryTypeAsync(Guid portfolioId, string categoryType);
        Task<List<Skill>> GetSkillsBySubcategoryAsync(Guid portfolioId, string subcategory);
    }
}
