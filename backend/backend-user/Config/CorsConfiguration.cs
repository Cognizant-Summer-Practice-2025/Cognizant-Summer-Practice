namespace backend_user.Config;

/// <summary>
/// Configuration class for CORS policy setup 
/// </summary>
public static class CorsConfiguration
{
    public const string FrontendPolicyName = "AllowFrontend";

    /// <summary>
    /// Configures CORS policies for frontend services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendUrls = GetFrontendUrls(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicyName, policy =>
            {
                policy.WithOrigins(frontendUrls)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Gets frontend URLs from configuration with fallback to default development URLs
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Array of allowed frontend URLs</returns>
    private static string[] GetFrontendUrls(IConfiguration configuration)
    {
        var configuredUrls = configuration.GetSection("FrontendUrls").Get<string[]>();
        
        if (configuredUrls?.Length > 0)
        {
            return configuredUrls;
        }

        // Fallback to default development URLs
        return new[]
        {
            "http://localhost:3000",  // auth-user-service
            "http://localhost:3001",  // home-portfolio-service
            "http://localhost:3002",  // messages-service
            "http://localhost:3003"   // admin-service
        };
    }
}

