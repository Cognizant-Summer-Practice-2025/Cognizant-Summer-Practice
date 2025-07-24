using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_messages.Models
{
    [Table("messages")]
    public class Message
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("conversation_id")]
        public Guid ConversationId { get; set; }

        [Required]
        [Column("sender_id")]
        public Guid SenderId { get; set; }

        [Column("reply_to_message_id")]
        public Guid? ReplyToMessageId { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("message_type")]
        public string MessageType { get; set; } = "Text";

        [Column("attachment_url")]
        public string? AttachmentUrl { get; set; }

        [Column("attachment_filename")]
        public string? AttachmentFilename { get; set; }

        [Column("attachment_size")]
        public int? AttachmentSize { get; set; }

        [Column("is_edited")]
        public bool IsEdited { get; set; } = false;

        [Column("edited_at")]
        public DateTime? EditedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public virtual Conversation? Conversation { get; set; }
        public virtual Message? ReplyToMessage { get; set; }
        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
    }
}
