using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;
using backend_user.DTO.User.Response;
using backend_user.Models;

namespace backend_user.Services.Mappers
{
    public class UserReportMapper : IUserReportMapper
    {
        public UserReport MapToEntity(UserReportCreateRequestDto request)
        {
            return new UserReport
            {
                UserId = request.UserId,
                ReportedByUserId = request.ReportedByUserId,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };
        }

        public UserReportResponseDto MapToResponseDto(UserReport userReport)
        {
            return new UserReportResponseDto
            {
                Id = userReport.Id,
                UserId = userReport.UserId,
                ReportedByUserId = userReport.ReportedByUserId,
                Reason = userReport.Reason,
                CreatedAt = userReport.CreatedAt
            };
        }

        public UserReportSummaryDto MapToSummaryDto(UserReport userReport)
        {
            return new UserReportSummaryDto
            {
                Id = userReport.Id,
                UserId = userReport.UserId,
                ReportedByUserId = userReport.ReportedByUserId,
                Reason = userReport.Reason,
                CreatedAt = userReport.CreatedAt
            };
        }

        public UserReportWithDetailsDto MapToWithDetailsDto(UserReport userReport)
        {
            return new UserReportWithDetailsDto
            {
                Id = userReport.Id,
                UserId = userReport.UserId,
                ReportedByUserId = userReport.ReportedByUserId,
                User = userReport.User != null ? new UserSummaryDto
                {
                    Id = userReport.User.Id,
                    Email = userReport.User.Email,
                    Username = userReport.User.Username,
                    FirstName = userReport.User.FirstName,
                    LastName = userReport.User.LastName,
                    ProfessionalTitle = userReport.User.ProfessionalTitle,
                    Location = userReport.User.Location,
                    AvatarUrl = userReport.User.AvatarUrl,
                    IsActive = userReport.User.IsActive,
                    CreatedAt = userReport.User.CreatedAt
                } : new UserSummaryDto(),
                ReportedByUser = new UserSummaryDto(), // This will be populated in the service
                Reason = userReport.Reason,
                CreatedAt = userReport.CreatedAt
            };
        }
    }
}
