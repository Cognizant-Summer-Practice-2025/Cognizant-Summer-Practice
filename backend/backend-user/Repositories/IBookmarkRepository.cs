using backend_user.Models;

namespace backend_user.Repositories
{
    public interface IBookmarkRepository
    {
        Task<Bookmark> AddBookmark(Bookmark bookmark);
        Task<bool> RemoveBookmark(Guid userId, string portfolioId);
        Task<IEnumerable<Bookmark>> GetUserBookmarks(Guid userId);
        Task<bool> IsBookmarked(Guid userId, string portfolioId);
        Task<Bookmark?> GetBookmark(Guid userId, string portfolioId);
    }
} 