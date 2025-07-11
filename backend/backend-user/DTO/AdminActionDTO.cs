using backend_user.Models;

namespace backend_user.DTO;

// AdminAction Response DTO
public class AdminActionResponseDto
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public string TargetService { get; set; } = string.Empty;
    public TargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ActionType ActionType { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserProfileDto? Admin { get; set; }
}

// AdminAction Request DTO
public class AdminActionRequestDto
{
    public Guid AdminId { get; set; }
    public string TargetService { get; set; } = string.Empty;
    public TargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ActionType ActionType { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

// AdminAction Summary DTO
public class AdminActionSummaryDto
{
    public Guid Id { get; set; }
    public string TargetService { get; set; } = string.Empty;
    public TargetType TargetType { get; set; }
    public ActionType ActionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AdminName { get; set; } = string.Empty;
}