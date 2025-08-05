using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendMessages.Models
{
    [Table("message_reports")]
    public class MessageReport
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Message")]
        public Guid MessageId { get; set; }

        [Required]
        public Guid ReportedByUserId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Message Message { get; set; } = null!;
    }
} 