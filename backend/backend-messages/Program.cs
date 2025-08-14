using BackendMessages.Config;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load();

// Configure services using extension methods
builder.Services
    .AddJsonConfiguration()
    .AddApiDocumentation()
    .AddSignalRConfiguration()
    .AddCorsConfiguration(builder.Configuration)
    .AddDatabaseServices(builder.Configuration)
    .AddHttpClientConfiguration(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

// Configure middleware pipeline
app.UseApplicationMiddleware();

// Map SignalR hubs
app.MapSignalRHubs();

app.Run();