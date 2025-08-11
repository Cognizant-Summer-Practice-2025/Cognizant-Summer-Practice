using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;
using backend_user.Models;

namespace backend_user.Services.Mappers
{
    public interface IUserReportMapper
    {
        UserReport MapToEntity(UserReportCreateRequestDto request);
        UserReportResponseDto MapToResponseDto(UserReport userReport);
        UserReportSummaryDto MapToSummaryDto(UserReport userReport);
        UserReportWithDetailsDto MapToWithDetailsDto(UserReport userReport);
    }
}
