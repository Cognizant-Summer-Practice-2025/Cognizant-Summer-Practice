using BackendMessages.Models;
using BackendMessages.Models.Email;

namespace BackendMessages.Services.Abstractions
{
    public interface IEmailService
    {
        Task<bool> SendUnreadMessagesNotificationAsync(UnreadMessagesNotification notification);
        Task<bool> SendContactRequestNotificationAsync(ContactRequestNotification notification);
    }
} 