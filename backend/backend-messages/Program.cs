using BackendMessages.Config;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file located in the project content root
DotNetEnv.Env.Load(Path.Combine(builder.Environment.ContentRootPath, ".env"));

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

// Log scheduler startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== MESSAGES SERVICE STARTUP ===");
logger.LogInformation("Application starting up at {StartupTime} UTC", DateTime.UtcNow);

// Log scheduler configuration
var configuration = app.Services.GetRequiredService<IConfiguration>();
var notificationTime = configuration["Scheduler:DailyNotificationTime"] ?? "18:00";
logger.LogInformation("Scheduler configured for daily notifications at {NotificationTime}", notificationTime);

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
    
    var emailUsername = Environment.GetEnvironmentVariable("GMAIL_USERNAME");
    var emailPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD");
    
    if (!string.IsNullOrEmpty(emailUsername) && !string.IsNullOrEmpty(emailPassword))
    {
        emailConfig["Email:SmtpUsername"] = emailUsername;
        emailConfig["Email:SmtpPassword"] = emailPassword;
        emailConfig["Email:FromAddress"] = emailUsername;
        
        Console.WriteLine($"✅ Email configured from environment variables: {emailUsername}");
    }
    else
    {
        Console.WriteLine("⚠️  Email environment variables not found. Using appsettings.json configuration.");
        Console.WriteLine("   To use Gmail: Set GMAIL_USERNAME and GMAIL_APP_PASSWORD environment variables.");
    }
    
    return emailConfig;
}