using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;

namespace backend_portfolio.Services.Abstractions
{
    public interface IPortfolioTemplateService
    {
        Task<IEnumerable<PortfolioTemplateResponse>> GetAllTemplatesAsync();
        Task<IEnumerable<PortfolioTemplateResponse>> GetActiveTemplatesAsync();
        Task<PortfolioTemplateResponse?> GetTemplateByIdAsync(Guid id);
        Task<PortfolioTemplateResponse?> GetTemplateByNameAsync(string name);
        Task<PortfolioTemplateResponse> CreateTemplateAsync(PortfolioTemplateCreateRequest request);
        Task<PortfolioTemplateResponse?> UpdateTemplateAsync(Guid id, PortfolioTemplateUpdateRequest request);
        Task<bool> DeleteTemplateAsync(Guid id);
        Task SeedDefaultTemplatesAsync();
    }
} 