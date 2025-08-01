
using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Repositories;
using BackendMessages.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddCors();

builder.Services.AddHttpClient<IUserSearchService, UserSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register repositories
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();

// Register services
builder.Services.AddScoped<IUserSearchService, UserSearchService>();
builder.Services.AddScoped<IConversationService, ConversationServiceRefactored>();
builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddDbContext<MessagesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MessagesDatabase")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHub<MessageHub>("/messagehub");

app.Run();