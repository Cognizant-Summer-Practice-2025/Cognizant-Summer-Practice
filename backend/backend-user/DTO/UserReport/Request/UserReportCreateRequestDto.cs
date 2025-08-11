using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO.UserReport.Request
{
    /// <summary>
    /// Request DTO for creating user reports.
    /// </summary>
    public class UserReportCreateRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ReportedByUserId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
