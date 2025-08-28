using backend_AI.Services;
using backend_AI.Services.Abstractions;
using backend_AI.Services.External;

namespace backend_AI.Config;

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
        services.AddAIServices();
        services.AddExternalServices();
        services.AddAuthenticationServices();

        return services;
    }

    /// <summary>
    /// Registers AI-related services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        services.AddScoped<IAiChatService, AiChatService>();
        services.AddScoped<IPortfolioRankingService, PortfolioRankingService>();

        return services;
    }

    /// <summary>
    /// Registers external service clients
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddScoped<IPortfolioApiClient, PortfolioApiClient>();
        services.AddScoped<ITechNewsPortfolioClient, TechNewsPortfolioClient>();

        return services;
    }

    /// <summary>
    /// Registers authentication and authorization services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        // Register typed HttpClient for user authentication service using the named client
        services.AddHttpClient<IUserAuthenticationService, UserAuthenticationService>(HttpClientConfiguration.UserServiceClientName);
        services.AddScoped<ISecurityHeadersService, SecurityHeadersService>();
        services.AddScoped<IAuthorizationPathService, AuthorizationPathService>();
        services.AddScoped<IAuthenticationStrategy, OAuth2AuthenticationStrategy>();
        services.AddScoped<IAuthenticationContextService, AuthenticationContextService>();

        return services;
    }
}
