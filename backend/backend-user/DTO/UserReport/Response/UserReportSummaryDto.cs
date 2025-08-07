using backend_user.Models;

namespace backend_user.DTO.UserReport.Response
{
    /// <summary>
    /// Summarized user report information for list views.
    /// </summary>
    public class UserReportSummaryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ReportedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
