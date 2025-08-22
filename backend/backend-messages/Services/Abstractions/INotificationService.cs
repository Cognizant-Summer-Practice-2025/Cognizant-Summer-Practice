using BackendMessages.DTO.Notification;

namespace BackendMessages.Services.Abstractions
{
    public interface INotificationService
    {
        Task<List<UnreadMessagesSummary>> GetUsersWithUnreadMessagesAsync();
        Task SendDailyUnreadMessagesNotificationsAsync();
    }
} 