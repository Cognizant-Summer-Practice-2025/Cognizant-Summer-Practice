using BackendMessages.Services.Jobs;
using Quartz;

namespace BackendMessages.Config;

/// <summary>
/// Configuration class for Quartz scheduler setup
/// </summary>
public static class SchedulerConfiguration
{
    /// <summary>
    /// Private class for logging purposes since static classes cannot be used as generic type parameters
    /// </summary>
    private class SchedulerConfigurationLogger { }

    /// <summary>
    /// Configures Quartz scheduler with daily unread messages notification job
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSchedulerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add logging service for configuration logging
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SchedulerConfigurationLogger>>();
        
        logger?.LogInformation("Configuring Quartz scheduler services...");

        // Add Quartz services
        services.AddQuartz(q =>
        {
            logger?.LogInformation("Setting up Quartz scheduler configuration");
            
            // Create a job key for our daily notification job
            var jobKey = new JobKey("DailyUnreadMessagesJob");
            
            // Configure the job
            q.AddJob<DailyUnreadMessagesJob>(opts => opts.WithIdentity(jobKey));
            logger?.LogInformation("Registered job: {JobKey}", jobKey);

            // Get the notification time from configuration (default to 18:00)
            var notificationTime = configuration["Scheduler:DailyNotificationTime"] ?? "18:00";
            var timeParts = notificationTime.Split(':');
            var hour = int.Parse(timeParts[0]);
            var minute = timeParts.Length > 1 ? int.Parse(timeParts[1]) : 0;

            var cronExpression = $"0 {minute} {hour} * * ?";
            logger?.LogInformation("Configuring daily notification job to run at {NotificationTime} (cron: {CronExpression})", 
                notificationTime, cronExpression);

            // Configure the trigger to run daily at the specified time
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("DailyUnreadMessagesJob-trigger")
                .WithCronSchedule(cronExpression) // Cron expression for daily at specified time
                .WithDescription("Trigger for daily unread messages notification job"));
                
            logger?.LogInformation("Configured trigger for job: {JobKey}", jobKey);
        });

        // Add the Quartz hosted service
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        
        logger?.LogInformation("Quartz scheduler services configured successfully. Scheduler will wait for jobs to complete on shutdown.");

        return services;
    }
} 