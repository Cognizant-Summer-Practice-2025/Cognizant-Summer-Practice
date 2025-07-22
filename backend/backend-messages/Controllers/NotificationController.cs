using Microsoft.AspNetCore.Mvc;
using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;
using backend_messages.Services;

namespace backend_messages.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<NotificationResponse>>> GetNotifications(Guid userId)
        {
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return Ok(notifications);
        }

        [HttpPost]
        public async Task<ActionResult<NotificationResponse>> CreateNotification(CreateNotificationRequest request)
        {
            var notification = await _notificationService.CreateNotificationAsync(request);
            return CreatedAtAction(nameof(GetNotifications), new { userId = notification.UserId }, notification);
        }
    }
}