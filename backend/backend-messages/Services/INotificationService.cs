using backend_messages.Models;
using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;

namespace backend_messages.Services
{
    public interface INotificationService
    {
        Task<List<NotificationResponse>> GetNotificationsByUserIdAsync(Guid userId);
        Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
    }
}