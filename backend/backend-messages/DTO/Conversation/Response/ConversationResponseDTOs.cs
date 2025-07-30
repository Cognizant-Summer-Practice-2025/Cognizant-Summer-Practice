using BackendMessages.DTO.Message.Response;

namespace BackendMessages.DTO.Conversation.Response
{
    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public Guid InitiatorId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserAvatar { get; set; }
        public string? OtherUserProfessionalTitle { get; set; }
        public DateTime LastMessageTimestamp { get; set; }
        public MessageSummaryResponse? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ConversationDetailResponse
    {
        public Guid Id { get; set; }
        public Guid InitiatorId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserAvatar { get; set; }
        public string? OtherUserProfessionalTitle { get; set; }
        public DateTime LastMessageTimestamp { get; set; }
        public MessageResponse? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsOnline { get; set; }
        public IEnumerable<MessageResponse> RecentMessages { get; set; } = new List<MessageResponse>();
    }

    public class ConversationsPagedResponse
    {
        public IEnumerable<ConversationResponse> Conversations { get; set; } = new List<ConversationResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class CreateConversationResponse
    {
        public ConversationResponse Conversation { get; set; } = new();
        public MessageResponse? InitialMessage { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class DeleteConversationResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class ConversationStatsResponse
    {
        public int TotalConversations { get; set; }
        public int UnreadConversations { get; set; }
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public DateTime? LastActivity { get; set; }
    }
} 