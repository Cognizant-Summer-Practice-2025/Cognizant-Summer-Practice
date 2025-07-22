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
    public interface IPortfolioTemplateRepository
    {
        Task<List<PortfolioTemplate>> GetAllTemplatesAsync();
        Task<PortfolioTemplate?> GetTemplateByIdAsync(Guid id);
        Task<PortfolioTemplate> CreateTemplateAsync(PortfolioTemplateCreateRequest request);
        Task<PortfolioTemplate?> UpdateTemplateAsync(Guid id, PortfolioTemplateUpdateRequest request);
        Task<bool> DeleteTemplateAsync(Guid id);
    }
}
