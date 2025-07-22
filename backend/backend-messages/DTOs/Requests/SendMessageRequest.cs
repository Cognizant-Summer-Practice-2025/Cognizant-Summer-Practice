namespace backend_messages.DTOs.Requests
{
    public class SendMessageRequest
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}