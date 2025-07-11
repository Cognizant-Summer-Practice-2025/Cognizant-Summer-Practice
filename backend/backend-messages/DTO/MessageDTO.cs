using backend_messages.Models;

namespace backend_messages.DTO
{
    // Message Response DTO
    public class MessageResponseDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFilename { get; set; }
        public int? AttachmentSize { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserCacheDto? Sender { get; set; }
        public MessageSummaryDto? ReplyToMessage { get; set; }
        public List<MessageReadSummaryDto> ReadBy { get; set; } = new();
        public int ReadCount { get; set; }
    }

    // Message Request DTO
    public class MessageRequestDto
    {
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFilename { get; set; }
        public int? AttachmentSize { get; set; }
    }

    // Message Summary DTO (for list views)
    public class MessageSummaryDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }
        public bool IsEdited { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserCacheDto? Sender { get; set; }
    }

    // Message Update DTO
    public class MessageUpdateDto
    {
        public string Content { get; set; } = string.Empty;
    }
} 