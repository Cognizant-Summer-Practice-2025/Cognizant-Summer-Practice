namespace backend_messages.DTOs.Responses
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 