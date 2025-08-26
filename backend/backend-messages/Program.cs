using BackendMessages.Config;
using BackendMessages.Models.Email;
using BackendMessages.Services;
using Microsoft.Extensions.Options;

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
    .AddSchedulerServices(builder.Configuration)
    .AddStartupValidation();

var app = builder.Build();

// Validate configuration at startup
using (var scope = app.Services.CreateScope())
{
    var validator = scope.ServiceProvider.GetRequiredService<IStartupValidationService>();
    validator.ValidateConfiguration();
}

app.UseApplicationMiddleware();

// Map SignalR hubs
app.MapSignalRHubs();

app.Run();
