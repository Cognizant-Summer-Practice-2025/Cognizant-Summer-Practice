using backend_portfolio.DTO.Request;
using backend_portfolio.DTO.Response;

namespace backend_portfolio.Services.Abstractions
{
    public interface IPortfolioCommandService
    {
        Task<PortfolioResponse> CreatePortfolioAsync(PortfolioCreateRequest request);
        Task<PortfolioResponse> CreatePortfolioAndGetIdAsync(PortfolioCreateRequest request);
        Task<BulkPortfolioContentResponse> SavePortfolioContentAsync(Guid portfolioId, BulkPortfolioContentRequest request);
        Task<PortfolioResponse?> UpdatePortfolioAsync(Guid id, PortfolioUpdateRequest request);
        Task<bool> DeletePortfolioAsync(Guid id);
        Task<bool> IncrementViewCountAsync(Guid id);
        Task<bool> IncrementLikeCountAsync(Guid id);
        Task<bool> DecrementLikeCountAsync(Guid id);
    }
} 