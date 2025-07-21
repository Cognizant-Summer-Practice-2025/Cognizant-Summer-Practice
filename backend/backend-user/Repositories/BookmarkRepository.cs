using backend_user.Data;
using backend_user.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_user.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly UserDbContext _context;

        public BookmarkRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Bookmark> AddBookmark(Bookmark bookmark)
        {
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();
            return bookmark;
        }

        public async Task<bool> RemoveBookmark(Guid userId, string portfolioId)
        {
            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.PortfolioId == portfolioId);
            
            if (bookmark == null)
                return false;

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Bookmark>> GetUserBookmarks(Guid userId)
        {
            return await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsBookmarked(Guid userId, string portfolioId)
        {
            return await _context.Bookmarks
                .AnyAsync(b => b.UserId == userId && b.PortfolioId == portfolioId);
        }

        public async Task<Bookmark?> GetBookmark(Guid userId, string portfolioId)
        {
            return await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.PortfolioId == portfolioId);
        }
    }
} 