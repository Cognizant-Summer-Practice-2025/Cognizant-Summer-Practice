using System.ComponentModel.DataAnnotations;
using System.Net;

namespace backend_user.DTO
{
    // Request DTO
    public class UserAnalyticsCreateRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;

        public string EventData { get; set; } = "{}";

        public IPAddress? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? ReferrerUrl { get; set; }
    }

    // Response DTO
    public class UserAnalyticsResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventData { get; set; } = "{}";
        public IPAddress? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ReferrerUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserAnalyticsSummaryDto
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
} 