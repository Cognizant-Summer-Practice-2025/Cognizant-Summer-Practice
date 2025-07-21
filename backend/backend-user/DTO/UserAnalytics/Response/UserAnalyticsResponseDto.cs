using System.Net;

namespace backend_user.DTO.UserAnalytics.Response
{
    /// <summary>
    /// Complete user analytics response DTO with all analytics information.
    /// </summary>
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
}
