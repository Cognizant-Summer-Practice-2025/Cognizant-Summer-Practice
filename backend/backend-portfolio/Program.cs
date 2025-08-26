using backend_portfolio.Config;

// Load environment variables from .env file 
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure services using extension methods following SOLID principles
builder.Services
    .AddJsonConfiguration()
    .AddApiDocumentation()
    .AddCorsConfiguration(builder.Configuration)
    .AddDatabaseServices(builder.Configuration)
    .AddHttpClientConfiguration(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

// Configure middleware pipeline
app.UseApplicationMiddleware();

app.Run();
