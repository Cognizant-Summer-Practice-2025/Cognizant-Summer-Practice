using BackendMessages.Services.Abstractions;
using Quartz;

namespace BackendMessages.Services.Jobs
{
    [DisallowConcurrentExecution]
    public class DailyUnreadMessagesJob : IJob
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<DailyUnreadMessagesJob> _logger;

        public DailyUnreadMessagesJob(INotificationService notificationService, ILogger<DailyUnreadMessagesJob> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _notificationService.SendDailyUnreadMessagesNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during twice daily unread messages notification");
                throw; 
            }
        }
    }
}
