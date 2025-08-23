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
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;
            
            _logger.LogInformation("=== TWICE DAILY UNREAD MESSAGES NOTIFICATION START ===");
            _logger.LogInformation("Job: {JobKey}, Trigger: {TriggerKey}", jobKey, triggerKey);
            _logger.LogInformation("Processing twice daily unread messages notification at: {StartTime} UTC", DateTime.UtcNow);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Calling notification service to process twice daily unread messages...");
                
                await _notificationService.SendDailyUnreadMessagesNotificationsAsync();
                
                stopwatch.Stop();
                _logger.LogInformation("Twice daily unread messages notification completed successfully");
                _logger.LogInformation("Job execution time: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                _logger.LogInformation("=== TWICE DAILY UNREAD MESSAGES NOTIFICATION END ===");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during twice daily unread messages notification after {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                _logger.LogError("Job: {JobKey}, Error: {ErrorMessage}", jobKey, ex.Message);
                _logger.LogError("=== TWICE DAILY UNREAD MESSAGES NOTIFICATION FAILED ===");
                throw; 
            }
        }
    }
} 