using backend_user.Models;

namespace backend_user.Repositories
{
    public interface IUserReportRepository
    {
        Task<UserReport> CreateUserReportAsync(UserReport userReport);
        Task<UserReport?> GetUserReportByIdAsync(Guid id);
        Task<List<UserReport>> GetUserReportsAsync(Guid userId);
        Task<List<UserReport>> GetReportsByReporterAsync(Guid reporterId);
        Task<bool> HasUserReportedUserAsync(Guid userId, Guid reportedByUserId);
        Task<List<UserReport>> GetAllUserReportsAsync();
    }
} 