
// Load environment variables from .env file (like other backends)
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

// HttpClient for calling Ollama
builder.Services.AddHttpClient<backend_AI.Services.Abstractions.IAiChatService, backend_AI.Services.AiChatService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});

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
app.UseAuthorization();

// No auth middleware for now (testing from Podman)
app.MapControllers();

app.Run();
