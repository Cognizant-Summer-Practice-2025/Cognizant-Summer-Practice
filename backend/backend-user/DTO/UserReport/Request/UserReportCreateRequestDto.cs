using System.ComponentModel.DataAnnotations;
using backend_user.Models;

namespace backend_user.DTO.UserReport.Request
{
    /// <summary>
    /// Request DTO for creating user reports.
    /// </summary>
    public class UserReportCreateRequestDto
    {
        [Required]
        public Guid ReporterId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReportedService { get; set; } = string.Empty;

        [Required]
        public ReportedType ReportedType { get; set; }

        [Required]
        public Guid ReportedId { get; set; }

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
