using backend_user.DTO.UserAnalytics.Request;
using backend_user.DTO.UserAnalytics.Response;
using backend_user.Models;

namespace backend_user.Services.Mappers
{
    public class UserAnalyticsMapper : IUserAnalyticsMapper
    {
        public UserAnalytics MapToEntity(UserAnalyticsCreateRequestDto request)
        {
            return new UserAnalytics
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                EventType = request.EventType,
                EventData = request.EventData,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                ReferrerUrl = request.ReferrerUrl,
                CreatedAt = DateTime.UtcNow
            };
        }

        public UserAnalyticsResponseDto MapToResponseDto(UserAnalytics userAnalytics)
        {
            return new UserAnalyticsResponseDto
            {
                Id = userAnalytics.Id,
                UserId = userAnalytics.UserId,
                SessionId = userAnalytics.SessionId,
                EventType = userAnalytics.EventType,
                EventData = userAnalytics.EventData,
                IpAddress = userAnalytics.IpAddress,
                UserAgent = userAnalytics.UserAgent,
                ReferrerUrl = userAnalytics.ReferrerUrl,
                CreatedAt = userAnalytics.CreatedAt
            };
        }

        public UserAnalyticsSummaryDto MapToSummaryDto(UserAnalytics userAnalytics)
        {
            return new UserAnalyticsSummaryDto
            {
                Id = userAnalytics.Id,
                SessionId = userAnalytics.SessionId,
                EventType = userAnalytics.EventType,
                CreatedAt = userAnalytics.CreatedAt
            };
        }
    }
}
