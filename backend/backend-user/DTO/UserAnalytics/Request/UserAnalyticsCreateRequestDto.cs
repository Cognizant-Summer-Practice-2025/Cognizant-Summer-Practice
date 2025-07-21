using System.ComponentModel.DataAnnotations;
using System.Net;

namespace backend_user.DTO.UserAnalytics.Request
{
    /// <summary>
    /// Request DTO for creating user analytics entries.
    /// </summary>
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
}
