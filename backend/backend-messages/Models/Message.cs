using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_messages.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Conversation")]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid SenderId { get; set; } // External reference to User Service

        [ForeignKey("ReplyToMessage")]
        public Guid? ReplyToMessageId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public MessageType MessageType { get; set; } = MessageType.Text;

        public string? AttachmentUrl { get; set; }

        public string? AttachmentFilename { get; set; }

        public int? AttachmentSize { get; set; }

        public bool IsEdited { get; set; } = false;

        public DateTime? EditedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Conversation Conversation { get; set; } = null!;
        public virtual Message? ReplyToMessage { get; set; }
        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
        public virtual ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
    }
}
