using backend_portfolio.Models;
using backend_portfolio.DTO;

namespace backend_portfolio.Repositories
{
    public interface IBookmarkRepository
    {
        Task<List<Bookmark>> GetAllBookmarksAsync();
        Task<Bookmark?> GetBookmarkByIdAsync(Guid id);
        Task<List<Bookmark>> GetBookmarksByUserIdAsync(Guid userId);
        Task<Bookmark> CreateBookmarkAsync(BookmarkRequestDto request);
        Task<Bookmark?> UpdateBookmarkAsync(Guid id, BookmarkUpdateDto request);
        Task<bool> DeleteBookmarkAsync(Guid id);
        Task<bool> DeleteBookmarkByUserAndPortfolioAsync(Guid userId, Guid portfolioId);
        Task<bool> BookmarkExistsAsync(Guid userId, Guid portfolioId);
        Task<List<Bookmark>> GetBookmarksByCollectionAsync(Guid userId, string collectionName);
    }
}
