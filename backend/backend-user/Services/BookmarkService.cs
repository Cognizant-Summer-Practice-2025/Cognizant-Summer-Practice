using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;

namespace backend_user.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly IUserRepository _userRepository;

        public BookmarkService(
            IBookmarkRepository bookmarkRepository,
            IUserRepository userRepository)
        {
            _bookmarkRepository = bookmarkRepository ?? throw new ArgumentNullException(nameof(bookmarkRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<BookmarkResponse> AddBookmarkAsync(Guid userId, AddBookmarkRequest request)
        {
            // Check if user exists
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            // Check if already bookmarked
            var isBookmarked = await _bookmarkRepository.IsBookmarked(userId, request.PortfolioId);
            if (isBookmarked)
            {
                throw new InvalidOperationException("Portfolio is already bookmarked.");
            }

            var bookmark = new Bookmark
            {
                UserId = userId,
                PortfolioId = request.PortfolioId,
                PortfolioTitle = request.PortfolioTitle,
                PortfolioOwnerName = request.PortfolioOwnerName
            };

            var createdBookmark = await _bookmarkRepository.AddBookmark(bookmark);

            return new BookmarkResponse
            {
                Id = createdBookmark.Id,
                UserId = createdBookmark.UserId,
                PortfolioId = createdBookmark.PortfolioId,
                PortfolioTitle = createdBookmark.PortfolioTitle,
                PortfolioOwnerName = createdBookmark.PortfolioOwnerName,
                CreatedAt = createdBookmark.CreatedAt
            };
        }

        public async Task<bool> RemoveBookmarkAsync(Guid userId, string portfolioId)
        {
            // Check if user exists
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            var removed = await _bookmarkRepository.RemoveBookmark(userId, portfolioId);
            if (!removed)
            {
                throw new InvalidOperationException("Bookmark not found.");
            }

            return true;
        }

        public async Task<IEnumerable<BookmarkResponse>> GetUserBookmarksAsync(Guid userId)
        {
            // Check if user exists
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            var bookmarks = await _bookmarkRepository.GetUserBookmarks(userId);
            return bookmarks.Select(b => new BookmarkResponse
            {
                Id = b.Id,
                UserId = b.UserId,
                PortfolioId = b.PortfolioId,
                PortfolioTitle = b.PortfolioTitle,
                PortfolioOwnerName = b.PortfolioOwnerName,
                CreatedAt = b.CreatedAt
            });
        }

        public async Task<IsBookmarkedResponse> GetBookmarkStatusAsync(Guid userId, string portfolioId)
        {
            var isBookmarked = await _bookmarkRepository.IsBookmarked(userId, portfolioId);
            return new IsBookmarkedResponse { IsBookmarked = isBookmarked };
        }
    }
}
