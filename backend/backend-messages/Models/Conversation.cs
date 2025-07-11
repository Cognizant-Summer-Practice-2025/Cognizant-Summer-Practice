using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_messages.Models
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("LastMessage")]
        public Guid? LastMessageId { get; set; }

        [Required]
        public ConversationType Type { get; set; }

        public string? Title { get; set; }

        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Message? LastMessage { get; set; }
        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
} 