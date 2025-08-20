using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;
using backend_user.DTO.User.Response;
using backend_user.Services.Mappers;

namespace backend_user.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserReportMapper _userReportMapper;

        public UserReportService(IUserReportRepository userReportRepository, IUserRepository userRepository, IUserReportMapper userReportMapper)
        {
            _userReportRepository = userReportRepository;
            _userRepository = userRepository;
            _userReportMapper = userReportMapper;
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
            var userReport = _userReportMapper.MapToEntity(request);
            userReport.UserId = userId; // Override with the path parameter

            var createdReport = await _userReportRepository.CreateUserReportAsync(userReport);

            return _userReportMapper.MapToResponseDto(createdReport);
        }

        public async Task<UserReportResponseDto?> GetUserReportByIdAsync(Guid id)
        {
            var userReport = await _userReportRepository.GetUserReportByIdAsync(id);
            return userReport != null ? _userReportMapper.MapToResponseDto(userReport) : null;
        }

        public async Task<List<UserReportResponseDto>> GetUserReportsAsync(Guid userId)
        {
            var userReports = await _userReportRepository.GetUserReportsAsync(userId);
            return userReports.Select(ur => _userReportMapper.MapToResponseDto(ur)).ToList();
        }

        public async Task<List<UserReportResponseDto>> GetReportsByReporterAsync(Guid reporterId)
        {
            var userReports = await _userReportRepository.GetReportsByReporterAsync(reporterId);
            return userReports.Select(ur => _userReportMapper.MapToResponseDto(ur)).ToList();
        }

        public async Task<List<UserReportWithDetailsDto>> GetAllUserReportsAsync()
        {
            var userReports = await _userReportRepository.GetAllUserReportsAsync();
            var result = new List<UserReportWithDetailsDto>();
            
            foreach (var report in userReports)
            {
                var reportDto = _userReportMapper.MapToWithDetailsDto(report);
                
                // Get the reporter details
                var reporter = await _userRepository.GetUserById(report.ReportedByUserId);
                if (reporter != null)
                {
                    reportDto.ReportedByUser = new UserSummaryDto
                    {
                        Id = reporter.Id,
                        Email = reporter.Email,
                        Username = reporter.Username,
                        FirstName = reporter.FirstName,
                        LastName = reporter.LastName,
                        ProfessionalTitle = reporter.ProfessionalTitle,
                        Location = reporter.Location,
                        AvatarUrl = reporter.AvatarUrl,
                        IsActive = reporter.IsActive,
                        CreatedAt = reporter.CreatedAt
                    };
                }
                
                result.Add(reportDto);
            }
            
            return result;
        }
    }
} 