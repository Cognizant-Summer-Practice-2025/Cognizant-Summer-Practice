using backend_user.DTO.UserAnalytics.Request;
using backend_user.DTO.UserAnalytics.Response;

namespace backend_user.Services.Abstractions
{
    public interface IUserAnalyticsService
    {
        Task<UserAnalyticsResponseDto> CreateUserAnalyticsAsync(UserAnalyticsCreateRequestDto request);
        Task<UserAnalyticsResponseDto?> GetUserAnalyticsByIdAsync(Guid id);
        Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsAsync(Guid userId);
        Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsBySessionAsync(string sessionId);
        Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsByEventTypeAsync(string eventType);
        Task<List<UserAnalyticsResponseDto>> GetAllUserAnalyticsAsync();
        Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<int> GetUserAnalyticsCountAsync(Guid userId);
        Task<bool> DeleteUserAnalyticsAsync(Guid id);
        Task<List<UserAnalyticsSummaryDto>> GetUserAnalyticsSummaryAsync(Guid userId);
    }
}
