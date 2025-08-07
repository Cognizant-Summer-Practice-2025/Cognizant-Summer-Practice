using backend_user.DTO.UserAnalytics.Request;
using backend_user.DTO.UserAnalytics.Response;
using backend_user.Models;

namespace backend_user.Services.Mappers
{
    public interface IUserAnalyticsMapper
    {
        UserAnalytics MapToEntity(UserAnalyticsCreateRequestDto request);
        UserAnalyticsResponseDto MapToResponseDto(UserAnalytics userAnalytics);
        UserAnalyticsSummaryDto MapToSummaryDto(UserAnalytics userAnalytics);
    }
}
