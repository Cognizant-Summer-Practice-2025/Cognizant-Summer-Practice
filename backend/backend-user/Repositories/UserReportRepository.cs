using backend_user.Models;
using backend_user.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_user.Repositories
{
    public class UserReportRepository : IUserReportRepository
    {
        private readonly UserDbContext _context;

        public UserReportRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<UserReport> CreateUserReportAsync(UserReport userReport)
        {
            _context.UserReports.Add(userReport);
            await _context.SaveChangesAsync();
            return userReport;
        }

        public async Task<UserReport?> GetUserReportByIdAsync(Guid id)
        {
            return await _context.UserReports
                .Include(ur => ur.User)
                .FirstOrDefaultAsync(ur => ur.Id == id);
        }

        public async Task<List<UserReport>> GetUserReportsAsync(Guid userId)
        {
            return await _context.UserReports
                .Include(ur => ur.User)
                .Where(ur => ur.UserId == userId)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserReport>> GetReportsByReporterAsync(Guid reporterId)
        {
            return await _context.UserReports
                .Include(ur => ur.User)
                .Where(ur => ur.ReportedByUserId == reporterId)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserReportedUserAsync(Guid userId, Guid reportedByUserId)
        {
            return await _context.UserReports
                .AnyAsync(ur => ur.UserId == userId && ur.ReportedByUserId == reportedByUserId);
        }

        public async Task<List<UserReport>> GetAllUserReportsAsync()
        {
            return await _context.UserReports
                .Include(ur => ur.User)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }
    }
} 