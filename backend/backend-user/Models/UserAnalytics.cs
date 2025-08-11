using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace backend_user.Models
{
    [Table("user_analytics")]
    public class UserAnalytics
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Column(TypeName = "jsonb")]
        public string EventData { get; set; } = "{}";

        public IPAddress? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? ReferrerUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
} 