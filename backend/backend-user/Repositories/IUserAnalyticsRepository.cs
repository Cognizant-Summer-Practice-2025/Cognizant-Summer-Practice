using backend_user.Models;

namespace backend_user.Repositories
{
    public interface IUserAnalyticsRepository
    {
        Task<UserAnalytics> CreateUserAnalyticsAsync(UserAnalytics userAnalytics);
        Task<UserAnalytics?> GetUserAnalyticsByIdAsync(Guid id);
        Task<List<UserAnalytics>> GetUserAnalyticsAsync(Guid userId);
        Task<List<UserAnalytics>> GetUserAnalyticsBySessionAsync(string sessionId);
        Task<List<UserAnalytics>> GetUserAnalyticsByEventTypeAsync(string eventType);
        Task<List<UserAnalytics>> GetAllUserAnalyticsAsync();
        Task<List<UserAnalytics>> GetUserAnalyticsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<int> GetUserAnalyticsCountAsync(Guid userId);
        Task<bool> DeleteUserAnalyticsAsync(Guid id);
    }
}
