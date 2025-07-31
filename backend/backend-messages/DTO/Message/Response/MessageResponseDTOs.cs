using BackendMessages.Models;

namespace BackendMessages.DTO.Message.Response
{
    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public MessageResponse? ReplyToMessage { get; set; }
    }

    public class MessageSummaryResponse
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendMessageResponse
    {
        public MessageResponse Message { get; set; } = new();
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class MessagesPagedResponse
    {
        public IEnumerable<MessageResponse> Messages { get; set; } = new List<MessageResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class MarkAsReadResponse
    {
        public bool Success { get; set; }
        public int MessagesMarked { get; set; }
        public string? Error { get; set; }
    }

    public class DeleteMessageResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
} 