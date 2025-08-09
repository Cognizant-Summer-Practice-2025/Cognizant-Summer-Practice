using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.ImageUpload.Response;
using backend_portfolio.DTO.Pagination;

namespace backend_portfolio.Services.Abstractions
{
    public interface IPortfolioQueryService
    {
        Task<IEnumerable<PortfolioSummaryResponse>> GetAllPortfoliosAsync();
        Task<PortfolioDetailResponse?> GetPortfolioByIdAsync(Guid id);
        Task<IEnumerable<PortfolioSummaryResponse>> GetPortfoliosByUserIdAsync(Guid userId);
        Task<IEnumerable<PortfolioSummaryResponse>> GetPublishedPortfoliosAsync();
        Task<IEnumerable<PortfolioCardResponse>> GetPortfoliosForHomePageAsync();
        Task<PaginatedResponse<PortfolioCardResponse>> GetPortfoliosForHomePagePaginatedAsync(PaginationRequest request);
        Task<UserPortfolioComprehensiveResponse> GetUserPortfolioComprehensiveAsync(Guid userId);
        Task<IEnumerable<PortfolioDetailResponse>> GetAllPortfoliosDetailedAsync();
    }
} 