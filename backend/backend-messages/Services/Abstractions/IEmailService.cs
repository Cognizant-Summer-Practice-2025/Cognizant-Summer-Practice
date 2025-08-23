using BackendMessages.Models;

namespace BackendMessages.Services.Abstractions
{
    public interface IEmailService
    {
        Task<bool> SendUnreadMessagesNotificationAsync(string recipientEmail, string recipientName, int unreadCount, List<string> senderNames);
        Task<bool> SendMessageReceivedNotificationAsync(Message message, SearchUser recipient, SearchUser sender);
        Task<bool> SendContactRequestNotificationAsync(SearchUser recipient, SearchUser sender);
    }
} 