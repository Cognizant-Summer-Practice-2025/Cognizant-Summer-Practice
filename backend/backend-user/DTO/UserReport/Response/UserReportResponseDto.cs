using backend_user.Models;

namespace backend_user.DTO.UserReport.Response
{
    /// <summary>
    /// Complete user report response DTO with all report information.
    /// </summary>
    public class UserReportResponseDto
    {
        public Guid Id { get; set; }
        public Guid ReporterId { get; set; }
        public Guid? ResolvedBy { get; set; }
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
