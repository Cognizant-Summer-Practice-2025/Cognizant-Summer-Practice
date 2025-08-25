namespace BackendMessages.Models.Email
{
    public class ContactRequestNotification
    {
        public SearchUser Recipient { get; set; } = new();
        public SearchUser Sender { get; set; } = new();
    }
} 