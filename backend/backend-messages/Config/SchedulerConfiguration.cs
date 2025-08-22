using BackendMessages.Services.Jobs;
using Quartz;

namespace BackendMessages.Config;

/// <summary>
/// Configuration class for Quartz scheduler setup
/// </summary>
public static class SchedulerConfiguration
{
    /// <summary>
    /// Configures Quartz scheduler with daily unread messages notification job
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSchedulerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Quartz services
        services.AddQuartz(q =>
        {
            // Create a job key for our daily notification job
            var jobKey = new JobKey("DailyUnreadMessagesJob");
            
            // Configure the job
            q.AddJob<DailyUnreadMessagesJob>(opts => opts.WithIdentity(jobKey));

            // Get the notification time from configuration (default to 18:00)
            var notificationTime = configuration["Scheduler:DailyNotificationTime"] ?? "18:00";
            var timeParts = notificationTime.Split(':');
            var hour = int.Parse(timeParts[0]);
            var minute = timeParts.Length > 1 ? int.Parse(timeParts[1]) : 0;

            // Configure the trigger to run daily at the specified time
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("DailyUnreadMessagesJob-trigger")
                .WithCronSchedule($"0 {minute} {hour} * * ?") // Cron expression for daily at specified time
                .WithDescription("Trigger for daily unread messages notification job"));
        });

        // Add the Quartz hosted service
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
} 