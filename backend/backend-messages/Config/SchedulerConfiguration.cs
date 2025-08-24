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
            // Create a job key for our twice daily notification job
            var jobKey = new JobKey("TwiceDailyUnreadMessagesJob");          
            // Configure the job
            q.AddJob<DailyUnreadMessagesJob>(opts => opts.WithIdentity(jobKey));

            // Configure to run twice daily at 8:00 AM and 4:00 PM
            var cronExpression = "0 0 8,16 * * ?"; // At 08:00 and 16:00 every day

            // Configure the trigger to run twice daily
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("TwiceDailyUnreadMessagesJob-trigger")
                .WithCronSchedule(cronExpression) // Cron expression for twice daily
                .WithDescription("Trigger for twice daily unread messages notification job"));
                
        });

        // Add the Quartz hosted service
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        
        logger?.LogInformation("Quartz scheduler services configured successfully. Scheduler will wait for jobs to complete on shutdown.");

        return services;
    }
} 