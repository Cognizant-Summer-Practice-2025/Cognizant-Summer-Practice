using BackendMessages.Config;
using BackendMessages.Models.Email;
using Microsoft.Extensions.Options;
using System.IO;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

// Add configuration validation
builder.Services.AddSingleton<IValidateOptions<EmailSettings>, EmailSettingsValidator>();

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

// Validate critical email configuration using strongly-typed settings
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var emailSettings = app.Services.GetRequiredService<IOptions<EmailSettings>>().Value;

try
{
    emailSettings.Validate();

    var hasUsername = !string.IsNullOrEmpty(emailSettings.SmtpUsername);
    var hasPassword = !string.IsNullOrEmpty(emailSettings.SmtpPassword);
}
catch (Exception ex)
{
    logger.LogError(ex, "Email configuration validation failed: {Message}", ex.Message);
    
    // In production, you might want to exit the application if email config is invalid
    if (app.Environment.IsProduction())
    {
        logger.LogCritical("Application cannot start with invalid email configuration in production");
        Environment.Exit(1);
    }
}

app.UseApplicationMiddleware();

// Map SignalR hubs
app.MapSignalRHubs();

app.Run();

/// <summary>
/// Validator for EmailSettings configuration
/// </summary>
public class EmailSettingsValidator : IValidateOptions<EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, EmailSettings options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (Exception ex)
        {
            return ValidateOptionsResult.Fail(ex.Message);
        }
    }
}