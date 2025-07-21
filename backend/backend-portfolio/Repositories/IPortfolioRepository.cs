using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Request;

namespace backend_portfolio.Repositories
{
    public interface IPortfolioRepository
    {
        Task<List<Portfolio>> GetAllPortfoliosAsync();
        Task<Portfolio?> GetPortfolioByIdAsync(Guid id);
        Task<List<Portfolio>> GetPortfoliosByUserIdAsync(Guid userId);
        Task<Portfolio> CreatePortfolioAsync(PortfolioCreateRequest request);
        Task<Portfolio?> UpdatePortfolioAsync(Guid id, PortfolioUpdateRequest request);
        Task<bool> DeletePortfolioAsync(Guid id);
        Task<List<Portfolio>> GetPublishedPortfoliosAsync();
        Task<List<Portfolio>> GetPortfoliosByVisibilityAsync(Visibility visibility);
        Task<bool> IncrementViewCountAsync(Guid id);
        Task<bool> IncrementLikeCountAsync(Guid id);
        Task<bool> DecrementLikeCountAsync(Guid id);
    }
}
