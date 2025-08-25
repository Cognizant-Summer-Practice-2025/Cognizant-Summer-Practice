
using backend_AI.Config;

// Load environment variables from .env file (for local development)
// In production, environment variables should be set directly
DotNetEnv.Env.Load(overrideExistingVars: false);

var builder = WebApplication.CreateBuilder(args);

// Configure logging with environment variable support
builder.AddLoggingConfiguration();

// Configure services using extension methods
builder.Services
    .AddJsonConfiguration()
    .AddApiDocumentation()
    .AddCorsConfiguration(builder.Configuration)
    .AddHttpClientConfiguration(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

// Configure middleware pipeline
app.UseApplicationMiddleware();

app.Run();
