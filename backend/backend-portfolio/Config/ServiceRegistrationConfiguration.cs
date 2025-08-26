using backend_portfolio.Repositories;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;
using backend_portfolio.Services.Validators;
using backend_portfolio.Services.External;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;

namespace backend_portfolio.Config;

/// <summary>
/// Configuration class for dependency injection registration 
/// </summary>
public static class ServiceRegistrationConfiguration
{
    /// <summary>
    /// Registers all application services with dependency injection container
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddRepositoryServices();
        services.AddMapperServices();
        services.AddValidationServices();
        services.AddBusinessServices();
        services.AddAuthenticationServices();
        services.AddCacheServices();

        return services;
    }

    /// <summary>
    /// Registers repository services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IPortfolioTemplateRepository, PortfolioTemplateRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IExperienceRepository, ExperienceRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<ITechNewsSummaryRepository, TechNewsSummaryRepository>();

        return services;
    }

    /// <summary>
    /// Registers mapper services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddMapperServices(this IServiceCollection services)
    {
        services.AddScoped<IPortfolioMapper, PortfolioMapper>();
        services.AddScoped<IProjectMapper, ProjectMapper>();

        return services;
    }

    /// <summary>
    /// Registers validation services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddScoped<IValidationService<PortfolioCreateRequest>, PortfolioValidator>();
        services.AddScoped<IValidationService<PortfolioUpdateRequest>, PortfolioUpdateValidator>();
        services.AddScoped<IValidationService<ProjectCreateRequest>, ProjectValidator>();
        services.AddScoped<IValidationService<ProjectUpdateRequest>, ProjectUpdateValidator>();

        return services;
    }

    /// <summary>
    /// Registers business services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IExternalUserService, ExternalUserService>();
        services.AddScoped<IImageUploadUtility, ImageUploadUtility>();
        services.AddScoped<IPortfolioQueryService, PortfolioQueryService>();
        services.AddScoped<IPortfolioCommandService, PortfolioCommandService>();
        services.AddScoped<IPortfolioTemplateService, PortfolioTemplateService>();
        services.AddScoped<IProjectQueryService, ProjectQueryService>();
        services.AddScoped<IProjectCommandService, ProjectCommandService>();
        services.AddScoped<ITechNewsSummaryService, TechNewsSummaryService>();
        services.AddScoped<IHealthCheckService, HealthCheckService>();
        services.AddScoped<IAirflowAuthorizationService, AirflowAuthorizationService>();

        return services;
    }

    /// <summary>
    /// Registers authentication and authorization services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        services.AddScoped<ISecurityHeadersService, SecurityHeadersService>();
        services.AddScoped<IAuthorizationPathService, AuthorizationPathService>();
        services.AddScoped<IAuthenticationStrategy, OAuth2AuthenticationStrategy>();
        services.AddScoped<IAuthenticationContextService, AuthenticationContextService>();

        return services;
    }

    /// <summary>
    /// Registers caching services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddCacheServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        return services;
    }
}
