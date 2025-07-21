using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Request;

namespace backend_portfolio.Repositories
{
    public interface IPortfolioTemplateRepository
    {
        Task<List<PortfolioTemplate>> GetAllTemplatesAsync();
        Task<PortfolioTemplate?> GetTemplateByIdAsync(Guid id);
        Task<PortfolioTemplate> CreateTemplateAsync(PortfolioTemplateCreateRequest request);
        Task<PortfolioTemplate?> UpdateTemplateAsync(Guid id, PortfolioTemplateUpdateRequest request);
        Task<bool> DeleteTemplateAsync(Guid id);
    }
}
