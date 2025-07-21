using backend_portfolio.DTO.Response;

namespace backend_portfolio.Services.Abstractions
{
    public interface IPortfolioQueryService
    {
        Task<IEnumerable<PortfolioSummaryResponse>> GetAllPortfoliosAsync();
        Task<PortfolioDetailResponse?> GetPortfolioByIdAsync(Guid id);
        Task<IEnumerable<PortfolioSummaryResponse>> GetPortfoliosByUserIdAsync(Guid userId);
        Task<IEnumerable<PortfolioSummaryResponse>> GetPublishedPortfoliosAsync();
        Task<IEnumerable<PortfolioCardResponse>> GetPortfoliosForHomePageAsync();
        Task<UserPortfolioComprehensiveResponse> GetUserPortfolioComprehensiveAsync(Guid userId);
    }
} 