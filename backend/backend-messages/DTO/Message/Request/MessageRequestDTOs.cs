using System.ComponentModel.DataAnnotations;

namespace BackendMessages.DTO.Message.Request
{
    public class SendMessageRequest
    {
        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public Guid? ReplyToMessageId { get; set; }
    }

    public class MarkMessageAsReadRequest
    {
        [Required]
        public Guid MessageId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class MarkMessagesAsReadRequest
    {
        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class DeleteMessageRequest
    {
        [Required]
        public Guid MessageId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class ReportMessageRequest
    {
        [Required]
        public Guid MessageId { get; set; }

        [Required]
        public Guid ReportedById { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;
    }
} 