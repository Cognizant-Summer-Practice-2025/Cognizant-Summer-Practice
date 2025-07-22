using Microsoft.EntityFrameworkCore;
using backend_portfolio.Data;
using backend_portfolio.Repositories;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;
using backend_portfolio.Services.Validators;
using backend_portfolio.Services.External;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Request;
using backend_portfolio.DTO.Response;
using backend_portfolio.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseNpgsql(dataSource)
           .UseSnakeCaseNamingConvention());

builder.Services.AddHttpClient();

builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IPortfolioTemplateRepository, PortfolioTemplateRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IExperienceRepository, ExperienceRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();

builder.Services.AddScoped<PortfolioMapper>();
builder.Services.AddScoped<ProjectMapper>();

builder.Services.AddScoped<IValidationService<PortfolioCreateRequest>, PortfolioValidator>();
builder.Services.AddScoped<IValidationService<PortfolioUpdateRequest>, PortfolioUpdateValidator>();
builder.Services.AddScoped<IValidationService<ProjectCreateRequest>, ProjectValidator>();
builder.Services.AddScoped<IValidationService<ProjectUpdateRequest>, ProjectUpdateValidator>();

builder.Services.AddScoped<IExternalUserService, ExternalUserService>();

builder.Services.AddScoped<ImageUploadUtility>();

builder.Services.AddScoped<IPortfolioQueryService, PortfolioQueryService>();
builder.Services.AddScoped<IPortfolioCommandService, PortfolioCommandService>();
builder.Services.AddScoped<IProjectQueryService, ProjectQueryService>();
builder.Services.AddScoped<IProjectCommandService, ProjectCommandService>();



builder.Services.AddSingleton(dataSource);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Portfolio API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();
