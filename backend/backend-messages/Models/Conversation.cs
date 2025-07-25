using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendMessages.Models
{
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid InitiatorId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        public DateTime LastMessageTimestamp { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        public Guid? LastMessageId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        // Soft delete fields - track which users have "deleted" this conversation
        public DateTime? InitiatorDeletedAt { get; set; }
        public DateTime? ReceiverDeletedAt { get; set; }

        // Navigation properties
        [ForeignKey("LastMessageId")]
        public virtual Message? LastMessage { get; set; }
        
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        // Helper methods
        public bool IsDeletedByUser(Guid userId)
        {
            if (userId == InitiatorId)
                return InitiatorDeletedAt.HasValue;
            if (userId == ReceiverId)
                return ReceiverDeletedAt.HasValue;
            return false;
        }

        public bool IsDeletedByBothUsers()
        {
            return InitiatorDeletedAt.HasValue && ReceiverDeletedAt.HasValue;
        }
    }
} 