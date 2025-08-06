using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;

namespace backend_user.Services.Abstractions
{
    public interface IUserReportService
    {
        Task<UserReportResponseDto> CreateUserReportAsync(Guid userId, UserReportCreateRequestDto request);
        Task<UserReportResponseDto?> GetUserReportByIdAsync(Guid id);
        Task<List<UserReportResponseDto>> GetUserReportsAsync(Guid userId);
        Task<List<UserReportResponseDto>> GetReportsByReporterAsync(Guid reporterId);
        Task<List<UserReportResponseDto>> GetAllUserReportsAsync();
    }
} 