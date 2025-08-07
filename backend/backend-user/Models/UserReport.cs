using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    [Table("user_reports")]
    public class UserReport
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required]
        public Guid ReportedByUserId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
} 