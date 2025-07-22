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

namespace backend_portfolio.Repositories
{
    public interface ISkillRepository
    {
        Task<List<Skill>> GetAllSkillsAsync();
        Task<Skill?> GetSkillByIdAsync(Guid id);
        Task<List<Skill>> GetSkillsByPortfolioIdAsync(Guid portfolioId);
        Task<Skill> CreateSkillAsync(SkillCreateRequest request);
        Task<Skill?> UpdateSkillAsync(Guid id, SkillUpdateRequest request);
        Task<bool> DeleteSkillAsync(Guid id);
        Task<List<Skill>> GetSkillsByCategoryAsync(Guid portfolioId, string category);
        Task<List<Skill>> GetSkillsByCategoryTypeAsync(Guid portfolioId, string categoryType);
        Task<List<Skill>> GetSkillsBySubcategoryAsync(Guid portfolioId, string subcategory);
    }
}
