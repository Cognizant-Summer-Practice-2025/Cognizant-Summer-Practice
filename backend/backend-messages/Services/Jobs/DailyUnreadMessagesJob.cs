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
                _logger.LogInformation("Starting daily unread messages notification job at {Time}", DateTime.UtcNow);
                
                await _notificationService.SendDailyUnreadMessagesNotificationsAsync();
                
                _logger.LogInformation("Completed daily unread messages notification job at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing daily unread messages notification job");
                throw; 
            }
        }
    }
} 