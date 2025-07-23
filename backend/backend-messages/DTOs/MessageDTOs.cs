namespace backend_messages.DTOs
{
    public class SendMessageRequest
    {
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ConversationId { get; set; }
    }

    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserAvatar { get; set; }
        public MessageResponse? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserForMessagingResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? ProfessionalTitle { get; set; }
    }
} 