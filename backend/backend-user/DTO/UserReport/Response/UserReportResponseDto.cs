using backend_user.Models;

namespace backend_user.DTO.UserReport.Response
{
    /// <summary>
    /// Complete user report response DTO with all report information.
    /// </summary>
    public class UserReportResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ReportedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
