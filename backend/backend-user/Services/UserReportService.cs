using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;

namespace backend_user.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IUserRepository _userRepository;

        public UserReportService(IUserReportRepository userReportRepository, IUserRepository userRepository)
        {
            _userReportRepository = userReportRepository;
            _userRepository = userRepository;
        }

        public async Task<UserReportResponseDto> CreateUserReportAsync(Guid userId, UserReportCreateRequestDto request)
        {
            // Validate that the user being reported exists
            var reportedUser = await _userRepository.GetUserById(userId);
            if (reportedUser == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            // Validate that the reporter exists
            var reporter = await _userRepository.GetUserById(request.ReportedByUserId);
            if (reporter == null)
            {
                throw new ArgumentException($"Reporter with ID {request.ReportedByUserId} not found");
            }

            // Check if the user has already been reported by this reporter
            var alreadyReported = await _userReportRepository.HasUserReportedUserAsync(userId, request.ReportedByUserId);
            if (alreadyReported)
            {
                throw new InvalidOperationException("You have already reported this user");
            }

            // Prevent users from reporting themselves
            if (userId == request.ReportedByUserId)
            {
                throw new InvalidOperationException("You cannot report yourself");
            }

            // Create the user report
            var userReport = new UserReport
            {
                UserId = userId,
                ReportedByUserId = request.ReportedByUserId,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };

            var createdReport = await _userReportRepository.CreateUserReportAsync(userReport);

            return new UserReportResponseDto
            {
                Id = createdReport.Id,
                UserId = createdReport.UserId,
                ReportedByUserId = createdReport.ReportedByUserId,
                Reason = createdReport.Reason,
                CreatedAt = createdReport.CreatedAt
            };
        }

        public async Task<UserReportResponseDto?> GetUserReportByIdAsync(Guid id)
        {
            var userReport = await _userReportRepository.GetUserReportByIdAsync(id);
            if (userReport == null)
            {
                return null;
            }

            return new UserReportResponseDto
            {
                Id = userReport.Id,
                UserId = userReport.UserId,
                ReportedByUserId = userReport.ReportedByUserId,
                Reason = userReport.Reason,
                CreatedAt = userReport.CreatedAt
            };
        }

        public async Task<List<UserReportResponseDto>> GetUserReportsAsync(Guid userId)
        {
            var userReports = await _userReportRepository.GetUserReportsAsync(userId);
            return userReports.Select(ur => new UserReportResponseDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                ReportedByUserId = ur.ReportedByUserId,
                Reason = ur.Reason,
                CreatedAt = ur.CreatedAt
            }).ToList();
        }

        public async Task<List<UserReportResponseDto>> GetReportsByReporterAsync(Guid reporterId)
        {
            var userReports = await _userReportRepository.GetReportsByReporterAsync(reporterId);
            return userReports.Select(ur => new UserReportResponseDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                ReportedByUserId = ur.ReportedByUserId,
                Reason = ur.Reason,
                CreatedAt = ur.CreatedAt
            }).ToList();
        }

        public async Task<List<UserReportResponseDto>> GetAllUserReportsAsync()
        {
            var userReports = await _userReportRepository.GetAllUserReportsAsync();
            return userReports.Select(ur => new UserReportResponseDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                ReportedByUserId = ur.ReportedByUserId,
                Reason = ur.Reason,
                CreatedAt = ur.CreatedAt
            }).ToList();
        }
    }
} 