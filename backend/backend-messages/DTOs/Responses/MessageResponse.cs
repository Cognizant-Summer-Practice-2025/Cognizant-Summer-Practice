namespace backend_messages.DTOs.Responses
{
    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid ConversationId { get; set; }
        public bool IsRead { get; set; }
    }
}