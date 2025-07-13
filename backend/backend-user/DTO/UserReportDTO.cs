using System.ComponentModel.DataAnnotations;
using backend_user.Models;

namespace backend_user.DTO
{
    // Request DTO
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

    public class UserReportUpdateRequestDto
    {
        public ReportStatus? Status { get; set; }
        public string? AdminNotes { get; set; }
        public Guid? ResolvedBy { get; set; }
    }

    // Response DTO
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

    public class UserReportSummaryDto
    {
        public Guid Id { get; set; }
        public ReportType ReportType { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

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