namespace backend_messages.DTO
{
    // Notification Response DTO
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid? RelatedConversationId { get; set; }
        public Guid? RelatedMessageId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ConversationSummaryDto? RelatedConversation { get; set; }
        public MessageSummaryDto? RelatedMessage { get; set; }
    }

    // Notification Request DTO
    public class NotificationRequestDto
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid? RelatedConversationId { get; set; }
        public Guid? RelatedMessageId { get; set; }
    }

    // Notification Summary DTO
    public class NotificationSummaryDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Notification Update DTO
    public class NotificationUpdateDto
    {
        public bool IsRead { get; set; }
    }
} 