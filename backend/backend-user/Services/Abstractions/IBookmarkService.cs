using backend_user.DTO;
using backend_user.Models;

namespace backend_user.Services.Abstractions
{
    public interface IBookmarkService
    {
        Task<BookmarkResponse> AddBookmarkAsync(Guid userId, AddBookmarkRequest request);
        Task<bool> RemoveBookmarkAsync(Guid userId, string portfolioId);
        Task<IEnumerable<BookmarkResponse>> GetUserBookmarksAsync(Guid userId);
        Task<IsBookmarkedResponse> GetBookmarkStatusAsync(Guid userId, string portfolioId);
    }
}
