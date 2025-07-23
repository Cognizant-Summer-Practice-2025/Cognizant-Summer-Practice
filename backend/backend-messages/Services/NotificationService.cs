using backend_messages.Data;
using backend_messages.Models;
using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace backend_messages.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MessageDbContext _context;

        public NotificationService(MessageDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationResponse>> GetNotificationsByUserIdAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            return notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Message = request.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            
            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}