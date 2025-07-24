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
        public DateTime LastMessageTimestamp { get; set; } = DateTime.UtcNow;

        public Guid? LastMessageId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("LastMessageId")]
        public virtual Message? LastMessage { get; set; }
        
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
} 