using backend_portfolio.Models;
using backend_portfolio.DTO;

namespace backend_portfolio.Repositories
{
    public interface IPortfolioTemplateRepository
    {
        Task<List<PortfolioTemplate>> GetAllTemplatesAsync();
        Task<PortfolioTemplate?> GetTemplateByIdAsync(Guid id);
        Task<PortfolioTemplate> CreateTemplateAsync(PortfolioTemplateRequestDto request);
        Task<PortfolioTemplate?> UpdateTemplateAsync(Guid id, PortfolioTemplateUpdateDto request);
        Task<bool> DeleteTemplateAsync(Guid id);
        Task<List<PortfolioTemplate>> GetActiveTemplatesAsync();
    }
}
