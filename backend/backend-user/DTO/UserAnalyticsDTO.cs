namespace backend_user.DTO;

// UserAnalytics Response DTO
public class UserAnalyticsResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ReferrerUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

// UserAnalytics Request DTO
public class UserAnalyticsRequestDto
{
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public string? UserAgent { get; set; }
    public string? ReferrerUrl { get; set; }
}

// UserAnalytics Summary DTO
public class UserAnalyticsSummaryDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}