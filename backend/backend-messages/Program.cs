using BackendMessages.Config;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file located in the project content root
DotNetEnv.Env.Load(Path.Combine(builder.Environment.ContentRootPath, ".env"));

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

// Configure middleware pipeline
app.UseApplicationMiddleware();

// Map SignalR hubs
app.MapSignalRHubs();

app.Run();