using BackendMessages.Models;

namespace BackendMessages.Services.Abstractions
{
    public interface IEmailService
    {
        Task<bool> SendUnreadMessagesNotificationAsync(string recipientEmail, string recipientName, int unreadCount, List<string> senderNames);
    }
} 