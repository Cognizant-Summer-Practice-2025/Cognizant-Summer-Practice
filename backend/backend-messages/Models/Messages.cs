using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendMessages.Models
{
    public enum MessageType
    {
        Text = 0,
        Image = 1,
        File = 2,
        Audio = 3,
        Video = 4,
        System = 5
    }

    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        public Guid? ReplyToMessageId { get; set; }
        
        [ForeignKey("ReplyToMessageId")]
        public virtual Message? ReplyToMessage { get; set; }

        public string? Content { get; set; }

        [Required]
        public MessageType MessageType { get; set; } = MessageType.Text;

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }
    }
}