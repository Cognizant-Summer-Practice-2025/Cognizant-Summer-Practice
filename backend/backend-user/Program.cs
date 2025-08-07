using Microsoft.EntityFrameworkCore;
using backend_user.Data;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Middleware;
using Npgsql;

// Load environment variables from .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
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

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.MapEnum<OAuthProviderType>("oauth_provider_type");
dataSourceBuilder.MapEnum<ReportedType>("reported_type");
dataSourceBuilder.MapEnum<ReportType>("report_type");
dataSourceBuilder.MapEnum<ReportStatus>("report_status");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(dataSource)
           .UseSnakeCaseNamingConvention());

// Add Repository services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOAuthProviderRepository, OAuthProviderRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();

// Add Business Logic Services (following SOLID principles)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IOAuthProviderService, OAuthProviderService>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IOAuth2Service, OAuth2Service>();

// Register data source for disposal
builder.Services.AddSingleton(dataSource);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "User API v1");
    });
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

// Use OAuth 2.0 middleware
app.UseMiddleware<OAuth2Middleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
