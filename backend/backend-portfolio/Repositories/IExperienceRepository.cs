using backend_portfolio.Models;
using backend_portfolio.DTO;

namespace backend_portfolio.Repositories
{
    public interface IExperienceRepository
    {
        Task<List<Experience>> GetAllExperienceAsync();
        Task<Experience?> GetExperienceByIdAsync(Guid id);
        Task<List<Experience>> GetExperienceByPortfolioIdAsync(Guid portfolioId);
        Task<Experience> CreateExperienceAsync(ExperienceRequestDto request);
        Task<Experience?> UpdateExperienceAsync(Guid id, ExperienceUpdateDto request);
        Task<bool> DeleteExperienceAsync(Guid id);
        Task<List<Experience>> GetCurrentExperienceAsync(Guid portfolioId);
    }
}
