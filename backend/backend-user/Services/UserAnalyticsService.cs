using backend_user.DTO.UserAnalytics.Request;
using backend_user.DTO.UserAnalytics.Response;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;

namespace backend_user.Services
{
    public class UserAnalyticsService : IUserAnalyticsService
    {
        private readonly IUserAnalyticsRepository _userAnalyticsRepository;
        private readonly IUserAnalyticsMapper _userAnalyticsMapper;

        public UserAnalyticsService(IUserAnalyticsRepository userAnalyticsRepository, IUserAnalyticsMapper userAnalyticsMapper)
        {
            _userAnalyticsRepository = userAnalyticsRepository;
            _userAnalyticsMapper = userAnalyticsMapper;
        }

        public async Task<UserAnalyticsResponseDto> CreateUserAnalyticsAsync(UserAnalyticsCreateRequestDto request)
        {
            var userAnalytics = _userAnalyticsMapper.MapToEntity(request);
            var createdAnalytics = await _userAnalyticsRepository.CreateUserAnalyticsAsync(userAnalytics);
            return _userAnalyticsMapper.MapToResponseDto(createdAnalytics);
        }

        public async Task<UserAnalyticsResponseDto?> GetUserAnalyticsByIdAsync(Guid id)
        {
            var userAnalytics = await _userAnalyticsRepository.GetUserAnalyticsByIdAsync(id);
            return userAnalytics != null ? _userAnalyticsMapper.MapToResponseDto(userAnalytics) : null;
        }

        public async Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsAsync(Guid userId)
        {
            var userAnalytics = await _userAnalyticsRepository.GetUserAnalyticsAsync(userId);
            return userAnalytics.Select(ua => _userAnalyticsMapper.MapToResponseDto(ua)).ToList();
        }

        public async Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsBySessionAsync(string sessionId)
        {
            var userAnalytics = await _userAnalyticsRepository.GetUserAnalyticsBySessionAsync(sessionId);
            return userAnalytics.Select(ua => _userAnalyticsMapper.MapToResponseDto(ua)).ToList();
        }

        public async Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsByEventTypeAsync(string eventType)
        {
            var userAnalytics = await _userAnalyticsRepository.GetUserAnalyticsByEventTypeAsync(eventType);
            return userAnalytics.Select(ua => _userAnalyticsMapper.MapToResponseDto(ua)).ToList();
        }

        public async Task<List<UserAnalyticsResponseDto>> GetAllUserAnalyticsAsync()
        {
            var userAnalytics = await _userAnalyticsRepository.GetAllUserAnalyticsAsync();
            return userAnalytics.Select(ua => _userAnalyticsMapper.MapToResponseDto(ua)).ToList();
        }

        public async Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var userAnalytics = await _userAnalyticsRepository.GetUserAnalyticsByDateRangeAsync(userId, startDate, endDate);
            return userAnalytics.Select(ua => _userAnalyticsMapper.MapToResponseDto(ua)).ToList();
        }

        public async Task<int> GetUserAnalyticsCountAsync(Guid userId)
        {
            return await _userAnalyticsRepository.GetUserAnalyticsCountAsync(userId);
        }

        public async Task<bool> DeleteUserAnalyticsAsync(Guid id)
        {
            return await _userAnalyticsRepository.DeleteUserAnalyticsAsync(id);
        }

        public async Task<List<UserAnalyticsSummaryDto>> GetUserAnalyticsSummaryAsync(Guid userId)
        {
            var userAnalytics = await _userAnalyticsRepository.GetUserAnalyticsAsync(userId);
            return userAnalytics.Select(ua => _userAnalyticsMapper.MapToSummaryDto(ua)).ToList();
        }
    }
}
