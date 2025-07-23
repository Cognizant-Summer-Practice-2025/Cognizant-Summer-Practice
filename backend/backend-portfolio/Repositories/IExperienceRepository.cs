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
    public interface IExperienceRepository
    {
        Task<List<Experience>> GetAllExperienceAsync();
        Task<Experience?> GetExperienceByIdAsync(Guid id);
        Task<List<Experience>> GetExperienceByPortfolioIdAsync(Guid portfolioId);
        Task<Experience> CreateExperienceAsync(ExperienceCreateRequest request);
        Task<Experience?> UpdateExperienceAsync(Guid id, ExperienceUpdateRequest request);
        Task<bool> DeleteExperienceAsync(Guid id);
        Task<List<Experience>> GetCurrentExperienceAsync(Guid portfolioId);
    }
}
