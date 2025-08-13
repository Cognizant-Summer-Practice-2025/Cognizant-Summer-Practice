
// Load environment variables from .env file (like other backends)
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Logging configuration: enable fine-grained control via env vars and dev defaults
builder.Logging.AddConsole();
var logLevelEnv = Environment.GetEnvironmentVariable("LOG_LEVEL");
if (!string.IsNullOrWhiteSpace(logLevelEnv) && Enum.TryParse<LogLevel>(logLevelEnv, true, out var minLevel))
{
    builder.Logging.SetMinimumLevel(minLevel);
}
// Allow overriding log level for AI chat service specifically via AI_LOG_LEVEL
var aiLogLevelEnv = Environment.GetEnvironmentVariable("AI_LOG_LEVEL");
if (!string.IsNullOrWhiteSpace(aiLogLevelEnv) && Enum.TryParse<LogLevel>(aiLogLevelEnv, true, out var aiLevel))
{
    builder.Logging.AddFilter("backend_AI.Services.AiChatService", aiLevel);
}
// Always show detailed ranking logs in Development, or if RANKING_LOG_LEVEL is set
var rankingLevelEnv = Environment.GetEnvironmentVariable("RANKING_LOG_LEVEL");
if (!string.IsNullOrWhiteSpace(rankingLevelEnv) && Enum.TryParse<LogLevel>(rankingLevelEnv, true, out var rankingLevel))
{
    builder.Logging.AddFilter("backend_AI.Services.PortfolioRankingService", rankingLevel);
}
else if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("backend_AI.Services.PortfolioRankingService", LogLevel.Debug);
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // auth-user-service
                "http://localhost:3001",  // home-portfolio-service
                "http://localhost:3002",  // messages-service
                "http://localhost:3003"   // admin-service
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// HttpClient for calling AI provider (OpenRouter)
builder.Services.AddHttpClient<backend_AI.Services.Abstractions.IAiChatService, backend_AI.Services.AiChatService>(client =>
{
    var timeoutEnv = Environment.GetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS")
                      ?? Environment.GetEnvironmentVariable("OLLAMA_TIMEOUT_SECONDS");
    var timeoutCfg = builder.Configuration["OpenRouter:TimeoutSeconds"]
                     ?? builder.Configuration["Ollama:TimeoutSeconds"];
    if (int.TryParse(timeoutEnv, out var envSeconds) && envSeconds > 0)
    {
        client.Timeout = TimeSpan.FromSeconds(envSeconds);
    }
    else if (int.TryParse(timeoutCfg, out var cfgSeconds) && cfgSeconds > 0)
    {
        client.Timeout = TimeSpan.FromSeconds(cfgSeconds);
    }
    else
    {
        client.Timeout = TimeSpan.FromMinutes(2);
    }
});

// HttpClient for calling portfolio backend
builder.Services.AddHttpClient<backend_AI.Services.External.IPortfolioApiClient, backend_AI.Services.External.PortfolioApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpContextAccessor();

// Ranking service
builder.Services.AddScoped<backend_AI.Services.Abstractions.IPortfolioRankingService, backend_AI.Services.PortfolioRankingService>();

// User auth service and middleware dependencies
builder.Services.AddHttpClient<backend_AI.Services.Abstractions.IUserAuthenticationService, backend_AI.Services.UserAuthenticationService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Add Authentication services
builder.Services.AddScoped<backend_AI.Services.Abstractions.ISecurityHeadersService, backend_AI.Services.SecurityHeadersService>();
builder.Services.AddScoped<backend_AI.Services.Abstractions.IAuthorizationPathService, backend_AI.Services.AuthorizationPathService>();
builder.Services.AddScoped<backend_AI.Services.Abstractions.IAuthenticationStrategy, backend_AI.Services.OAuth2AuthenticationStrategy>();
builder.Services.AddScoped<backend_AI.Services.Abstractions.IAuthenticationContextService, backend_AI.Services.AuthenticationContextService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "AI API v1");
    });
}

app.UseCors("AllowFrontend");

// Skip HTTPS redirection during local/container testing
app.UseMiddleware<backend_AI.Middleware.OAuth2Middleware>();
app.UseAuthorization();

// No auth middleware for now (testing from Podman)
app.MapControllers();

app.Run();
