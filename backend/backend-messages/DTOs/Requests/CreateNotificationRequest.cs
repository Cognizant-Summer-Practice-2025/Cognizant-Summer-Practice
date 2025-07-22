namespace backend_messages.DTOs.Requests
{
    public class CreateNotificationRequest
    {
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 