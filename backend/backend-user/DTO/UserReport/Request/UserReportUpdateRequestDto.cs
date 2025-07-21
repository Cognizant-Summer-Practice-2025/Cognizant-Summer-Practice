using backend_user.Models;

namespace backend_user.DTO.UserReport.Request
{
    /// <summary>
    /// Request DTO for updating user reports (admin operations).
    /// </summary>
    public class UserReportUpdateRequestDto
    {
        public ReportStatus? Status { get; set; }
        public string? AdminNotes { get; set; }
        public Guid? ResolvedBy { get; set; }
    }
}
