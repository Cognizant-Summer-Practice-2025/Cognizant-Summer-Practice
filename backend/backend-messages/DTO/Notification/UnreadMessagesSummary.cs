namespace BackendMessages.DTO.Notification
{
    public class UnreadMessagesSummary
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public List<string> SenderNames { get; set; } = new List<string>();
    }
} 