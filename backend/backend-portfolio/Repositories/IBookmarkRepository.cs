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
    public interface IBookmarkRepository
    {
        Task<List<Bookmark>> GetAllBookmarksAsync();
        Task<Bookmark?> GetBookmarkByIdAsync(Guid id);
        Task<List<Bookmark>> GetBookmarksByUserIdAsync(Guid userId);
        Task<List<Bookmark>> GetBookmarksByPortfolioIdAsync(Guid portfolioId);
        Task<Bookmark> CreateBookmarkAsync(BookmarkCreateRequest request);
        Task<Bookmark?> UpdateBookmarkAsync(Guid id, BookmarkUpdateRequest request);
        Task<bool> DeleteBookmarkAsync(Guid id);
        Task<bool> DeleteBookmarkByUserAndPortfolioAsync(Guid userId, Guid portfolioId);
        Task<bool> BookmarkExistsAsync(Guid userId, Guid portfolioId);
        Task<List<Bookmark>> GetBookmarksByCollectionAsync(Guid userId, string collectionName);
    }
}
