using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Request;

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
