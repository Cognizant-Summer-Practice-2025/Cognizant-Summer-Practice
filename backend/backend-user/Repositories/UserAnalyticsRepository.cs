using backend_user.Data;
using backend_user.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_user.Repositories
{
    public class UserAnalyticsRepository : IUserAnalyticsRepository
    {
        private readonly UserDbContext _context;

        public UserAnalyticsRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<UserAnalytics> CreateUserAnalyticsAsync(UserAnalytics userAnalytics)
        {
            _context.UserAnalytics.Add(userAnalytics);
            await _context.SaveChangesAsync();
            return userAnalytics;
        }

        public async Task<UserAnalytics?> GetUserAnalyticsByIdAsync(Guid id)
        {
            return await _context.UserAnalytics
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.Id == id);
        }

        public async Task<List<UserAnalytics>> GetUserAnalyticsAsync(Guid userId)
        {
            return await _context.UserAnalytics
                .Include(ua => ua.User)
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserAnalytics>> GetUserAnalyticsBySessionAsync(string sessionId)
        {
            return await _context.UserAnalytics
                .Include(ua => ua.User)
                .Where(ua => ua.SessionId == sessionId)
                .OrderBy(ua => ua.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserAnalytics>> GetUserAnalyticsByEventTypeAsync(string eventType)
        {
            return await _context.UserAnalytics
                .Include(ua => ua.User)
                .Where(ua => ua.EventType == eventType)
                .OrderByDescending(ua => ua.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserAnalytics>> GetAllUserAnalyticsAsync()
        {
            return await _context.UserAnalytics
                .Include(ua => ua.User)
                .OrderByDescending(ua => ua.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserAnalytics>> GetUserAnalyticsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _context.UserAnalytics
                .Include(ua => ua.User)
                .Where(ua => ua.UserId == userId && ua.CreatedAt >= startDate && ua.CreatedAt <= endDate)
                .OrderByDescending(ua => ua.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUserAnalyticsCountAsync(Guid userId)
        {
            return await _context.UserAnalytics
                .CountAsync(ua => ua.UserId == userId);
        }

        public async Task<bool> DeleteUserAnalyticsAsync(Guid id)
        {
            var analytics = await _context.UserAnalytics.FindAsync(id);
            if (analytics == null)
                return false;

            _context.UserAnalytics.Remove(analytics);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
