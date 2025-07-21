using backend_user.Models;
using backend_user.DTO.User.Response;

namespace backend_user.DTO.UserReport.Response
{
    /// <summary>
    /// Detailed user report response DTO with user information included.
    /// </summary>
    public class UserReportWithDetailsDto
    {
        public Guid Id { get; set; }
        public UserSummaryDto Reporter { get; set; } = new();
        public UserSummaryDto? ResolvedByUser { get; set; }
        public string ReportedService { get; set; } = string.Empty;
        public ReportedType ReportedType { get; set; }
        public Guid ReportedId { get; set; }
        public ReportType ReportType { get; set; }
        public string Description { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
