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
            
            _logger.LogInformation("=== SCHEDULER JOB EXECUTION START ===");
            _logger.LogInformation("Job: {JobKey}, Trigger: {TriggerKey}", jobKey, triggerKey);
            _logger.LogInformation("Job execution started at: {StartTime} UTC", DateTime.UtcNow);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Calling notification service to process daily unread messages...");
                
                await _notificationService.SendDailyUnreadMessagesNotificationsAsync();
                
                stopwatch.Stop();
                _logger.LogInformation("Daily unread messages notification job completed successfully");
                _logger.LogInformation("Job execution time: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                _logger.LogInformation("=== SCHEDULER JOB EXECUTION END ===");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "=== SCHEDULER JOB EXECUTION FAILED ===");
                _logger.LogError("Job failed after {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                _logger.LogError("Job: {JobKey}, Error: {ErrorMessage}", jobKey, ex.Message);
                throw; 
            }
        }
    }
} 