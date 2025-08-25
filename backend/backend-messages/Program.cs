using BackendMessages.Config;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file located in the project content root
DotNetEnv.Env.Load();

// Configure Email settings from environment variables
builder.Configuration.AddInMemoryCollection(GetEmailConfigurationFromEnvironment());

// Configure services using extension methods
builder.Services
    .AddJsonConfiguration()
    .AddApiDocumentation()
    .AddSignalRConfiguration()
    .AddCorsConfiguration(builder.Configuration)
    .AddDatabaseServices(builder.Configuration)
    .AddHttpClientConfiguration(builder.Configuration)
    .AddApplicationServices()
    .AddSchedulerServices(builder.Configuration);

var app = builder.Build();

// Validate critical email configuration
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var configuration = app.Services.GetRequiredService<IConfiguration>();

var smtpPassword = configuration["Email:SmtpPassword"];
if (string.IsNullOrEmpty(smtpPassword))
{
    logger.LogWarning("SMTP Password not configured");
}
else if (smtpPassword.Contains("REPLACE_WITH") || smtpPassword.Contains("YOUR_PASSWORD") || smtpPassword.Contains("PLACEHOLDER"))
{
    logger.LogError("SMTP Password appears to be a placeholder. Please configure a real Gmail App Password.");
}

// Log scheduler configuration
logger.LogInformation("Scheduler configured to send unread messages notifications twice daily at 8:00 AM and 4:00 PM UTC");

// Configure middleware pipeline
app.UseApplicationMiddleware();

// Map SignalR hubs
app.MapSignalRHubs();

logger.LogInformation("Messages service started successfully with scheduler enabled");
logger.LogInformation("=== MESSAGES SERVICE READY ===");

app.Run();

// Method to get email configuration from environment variables
static Dictionary<string, string?> GetEmailConfigurationFromEnvironment()
{
    var emailConfig = new Dictionary<string, string?>();
    
    // Add email configuration from environment variables if they exist
    var envVars = new[]
    {
        "EMAIL_SMTP_HOST",
        "EMAIL_SMTP_PORT", 
        "EMAIL_SMTP_USERNAME",
        "EMAIL_FROM_ADDRESS",
        "EMAIL_FROM_NAME",
        "EMAIL_USE_SSL",
        "EMAIL_ENABLE_CONTACT_NOTIFICATIONS"
    };

    foreach (var envVar in envVars)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(value))
        {
            var configKey = envVar.Replace("EMAIL_", "Email:").Replace("_", "");
            emailConfig[configKey] = value;
        }
    }

    // Handle Gmail-specific environment variables
    var gmailUsername = Environment.GetEnvironmentVariable("GMAIL_USERNAME");
    if (!string.IsNullOrEmpty(gmailUsername))
    {
        emailConfig["Email:SmtpUsername"] = gmailUsername;
    }
    
    var gmailAppPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD");
    if (!string.IsNullOrEmpty(gmailAppPassword))
    {
        emailConfig["Email:SmtpPassword"] = gmailAppPassword;
    }

    return emailConfig;
}