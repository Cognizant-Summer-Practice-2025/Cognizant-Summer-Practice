using BackendMessages.Models.Email;

namespace BackendMessages.Services.Abstractions
{
    public interface IEmailValidator
    {
        bool IsValidEmail(string email);
        bool IsValidUnreadMessagesNotification(UnreadMessagesNotification notification);
        bool IsValidContactRequestNotification(ContactRequestNotification notification);
    }
} 