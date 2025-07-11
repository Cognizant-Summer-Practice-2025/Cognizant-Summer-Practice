using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_messages.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // External reference to User Service

        [Required]
        public string Type { get; set; } = string.Empty; // e.g., "message", "mention", "conversation_invite"

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public Guid? RelatedConversationId { get; set; }

        public Guid? RelatedMessageId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("RelatedConversationId")]
        public virtual Conversation? RelatedConversation { get; set; }

        [ForeignKey("RelatedMessageId")]
        public virtual Message? RelatedMessage { get; set; }
    }
}
