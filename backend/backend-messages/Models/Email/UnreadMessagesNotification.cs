namespace BackendMessages.Models.Email
{
    public class UnreadMessagesNotification
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public List<string> SenderNames { get; set; } = new();
    }
} 