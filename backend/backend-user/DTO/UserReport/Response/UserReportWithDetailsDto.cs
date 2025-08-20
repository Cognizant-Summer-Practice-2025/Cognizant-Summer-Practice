using backend_user.DTO.User.Response;

namespace backend_user.DTO.UserReport.Response
{
    /// <summary>
    /// Detailed user report response DTO with user information included.
    /// </summary>
    public class UserReportWithDetailsDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ReportedByUserId { get; set; }
        public UserSummaryDto User { get; set; } = new();
        public UserSummaryDto ReportedByUser { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
