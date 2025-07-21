using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Request;
using Microsoft.EntityFrameworkCore;

namespace backend_portfolio.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly PortfolioDbContext _context;

        public BookmarkRepository(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<Bookmark>> GetAllBookmarksAsync()
        {
            return await _context.Bookmarks
                .Include(b => b.Portfolio)
                .ToListAsync();
        }

        public async Task<Bookmark?> GetBookmarkByIdAsync(Guid id)
        {
            return await _context.Bookmarks
                .Include(b => b.Portfolio)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Bookmark>> GetBookmarksByUserIdAsync(Guid userId)
        {
            return await _context.Bookmarks
                .Include(b => b.Portfolio)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Bookmark>> GetBookmarksByPortfolioIdAsync(Guid portfolioId)
        {
            return await _context.Bookmarks
                .Include(b => b.Portfolio)
                .Where(b => b.PortfolioId == portfolioId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Bookmark> CreateBookmarkAsync(BookmarkCreateRequest request)
        {
            var bookmark = new Bookmark
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PortfolioId = request.PortfolioId,
                CollectionName = request.CollectionName,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();
            return bookmark;
        }

        public async Task<Bookmark?> UpdateBookmarkAsync(Guid id, BookmarkUpdateRequest request)
        {
            var bookmark = await _context.Bookmarks.FindAsync(id);
            if (bookmark == null) return null;

            if (request.CollectionName != null) bookmark.CollectionName = request.CollectionName;
            if (request.Notes != null) bookmark.Notes = request.Notes;

            await _context.SaveChangesAsync();
            return bookmark;
        }

        public async Task<bool> DeleteBookmarkAsync(Guid id)
        {
            var bookmark = await _context.Bookmarks.FindAsync(id);
            if (bookmark == null) return false;

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBookmarkByUserAndPortfolioAsync(Guid userId, Guid portfolioId)
        {
            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.PortfolioId == portfolioId);
            
            if (bookmark == null) return false;

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BookmarkExistsAsync(Guid userId, Guid portfolioId)
        {
            return await _context.Bookmarks
                .AnyAsync(b => b.UserId == userId && b.PortfolioId == portfolioId);
        }

        public async Task<List<Bookmark>> GetBookmarksByCollectionAsync(Guid userId, string collectionName)
        {
            return await _context.Bookmarks
                .Include(b => b.Portfolio)
                .Where(b => b.UserId == userId && b.CollectionName == collectionName)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}
