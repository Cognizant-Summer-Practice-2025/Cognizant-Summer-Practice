using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_messages.Models
{
    public class ConversationParticipant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Conversation")]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid UserId { get; set; } // External reference to User Service

        [ForeignKey("LastReadMessage")]
        public Guid? LastReadMessageId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LeftAt { get; set; }

        public bool IsMuted { get; set; } = false;

        public ParticipantRole Role { get; set; } = ParticipantRole.Member;

        // Navigation properties
        public virtual Conversation Conversation { get; set; } = null!;
        public virtual Message? LastReadMessage { get; set; }
    }
} 