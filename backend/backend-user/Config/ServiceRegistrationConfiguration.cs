using backend_user.Repositories;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Extensions;

namespace backend_user.Config;

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
        services.AddBusinessServices();
        services.AddAuthenticationServices(); // Uses existing extension method

        return services;
    }

    /// <summary>
    /// Registers repository services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOAuthProviderRepository, OAuthProviderRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IUserReportRepository, UserReportRepository>();
        services.AddScoped<IUserAnalyticsRepository, UserAnalyticsRepository>();

        return services;
    }

    /// <summary>
    /// Registers mapper services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddMapperServices(this IServiceCollection services)
    {
        services.AddScoped<IUserAnalyticsMapper, UserAnalyticsMapper>();
        services.AddScoped<IUserReportMapper, UserReportMapper>();

        return services;
    }

    /// <summary>
    /// Registers business logic services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IOAuthProviderService, OAuthProviderService>();
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        services.AddScoped<IUserReportService, UserReportService>();
        services.AddScoped<IUserAnalyticsService, UserAnalyticsService>();
        services.AddScoped<IOAuth2Service, OAuth2Service>();

        return services;
    }
}
