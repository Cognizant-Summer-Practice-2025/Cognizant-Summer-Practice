using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using BackendMessages.Data;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Repositories;
using BackendMessages.Hubs;
using BackendMessages.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file (align with other services)
DotNetEnv.Env.Load();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

builder.Services.AddCors();

// Configure HttpClient with connection pooling and recycling for external services
builder.Services.AddHttpClient("UserService", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "MessagesService/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    MaxConnectionsPerServer = 10, // Limit concurrent connections per server
    UseCookies = false // Disable cookies for API calls
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Recycle handlers every 5 minutes

// Configure HttpClient factory with global pooling settings
builder.Services.Configure<HttpClientFactoryOptions>(options =>
{
    options.HandlerLifetime = TimeSpan.FromMinutes(5); // Global handler lifetime
});

// Configure default HttpClient factory for any other HTTP needs
builder.Services.AddHttpClient();

// Register repositories
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageReportRepository, MessageReportRepository>();

// Register services
builder.Services.AddScoped<IUserSearchService, UserSearchService>();
builder.Services.AddScoped<IConversationService, ConversationServiceRefactored>();
builder.Services.AddScoped<IMessageService, MessageService>();

// Add Authentication services
builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

builder.Services.AddDbContext<MessagesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MessagesDatabase")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Messages API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors(policy => policy
    .WithOrigins(
        "http://localhost:3000",  // auth-user-service
        "http://localhost:3001",  // home-portfolio-service
        "http://localhost:3002",  // messages-service
        "http://localhost:3003"   // admin-service
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

// Use OAuth 2.0 middleware
app.UseMiddleware<OAuth2Middleware>();

app.UseAuthorization();
app.MapControllers();

app.MapHub<MessageHub>("/messagehub");

app.Run();