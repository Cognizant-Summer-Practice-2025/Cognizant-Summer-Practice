using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace backend_user.Models
{
    public class UserAnalytics
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = string.Empty;

        public string? EventData { get; set; } // JSON stored as string

        public IPAddress? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? ReferrerUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
} 