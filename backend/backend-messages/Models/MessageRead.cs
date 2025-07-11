using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_messages.Models
{
    public class MessageRead
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Message")]
        public Guid MessageId { get; set; }

        [Required]
        public Guid UserId { get; set; } // External reference to User Service

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Message Message { get; set; } = null!;
    }
} 