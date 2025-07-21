using backend_user.Models;

namespace backend_user.DTO.UserReport.Response
{
    /// <summary>
    /// Summarized user report information for list views.
    /// </summary>
    public class UserReportSummaryDto
    {
        public Guid Id { get; set; }
        public ReportType ReportType { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
