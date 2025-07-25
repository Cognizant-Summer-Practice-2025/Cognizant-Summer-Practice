
using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Services;
using BackendMessages.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS - allow all for development
builder.Services.AddCors();

// Add HTTP Client for user service communication
builder.Services.AddHttpClient<IUserSearchService, UserSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register services
builder.Services.AddScoped<IUserSearchService, UserSearchService>();
builder.Services.AddScoped<IConversationService, ConversationService>();

// Add DB Context
builder.Services.AddDbContext<MessagesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MessagesDatabase")));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS for development
app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:3001") // Add your frontend URLs
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .WithExposedHeaders("*"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map SignalR hub
app.MapHub<MessageHub>("/messagehub");

app.Run();