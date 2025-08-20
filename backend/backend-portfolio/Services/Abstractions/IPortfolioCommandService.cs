using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.ImageUpload.Response;

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
        Task DeleteAllUserPortfolioDataAsync(Guid userId);
    }
} 