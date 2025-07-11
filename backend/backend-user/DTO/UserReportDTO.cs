using backend_user.Models;

namespace backend_user.DTO;

// UserReport Response DTO
public class UserReportResponseDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string ReportedService { get; set; } = string.Empty;
    public TargetType ReportedType { get; set; }
    public Guid ReportedId { get; set; }
    public ReportType ReportType { get; set; }
    public string Description { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserProfileDto? Reporter { get; set; }
    public UserProfileDto? Resolver { get; set; }
}

// UserReport Request DTO
public class UserReportRequestDto
{
    public Guid ReporterId { get; set; }
    public string ReportedService { get; set; } = string.Empty;
    public TargetType ReportedType { get; set; }
    public Guid ReportedId { get; set; }
    public ReportType ReportType { get; set; }
    public string Description { get; set; } = string.Empty;
}

// UserReport Update DTO
public class UserReportUpdateDto
{
    public Guid? ResolvedBy { get; set; }
    public ReportStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}

// UserReport Summary DTO
public class UserReportSummaryDto
{
    public Guid Id { get; set; }
    public string ReportedService { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ReporterName { get; set; } = string.Empty;
}