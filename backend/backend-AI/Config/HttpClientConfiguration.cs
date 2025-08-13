using Microsoft.Extensions.Http;

namespace backend_AI.Config;

/// <summary>
/// Configuration class for HTTP client services 
/// </summary>
public static class HttpClientConfiguration
{
    public const string AIProviderClientName = "AIProvider";
    public const string PortfolioServiceClientName = "PortfolioService";
    public const string UserServiceClientName = "UserService";

    /// <summary>
    /// Configures HTTP client services with connection pooling and recycling
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var httpClientSettings = GetHttpClientSettings(configuration);

        ConfigureAIProviderClient(services, configuration, httpClientSettings);
        ConfigurePortfolioServiceClient(services, httpClientSettings);
        ConfigureUserServiceClient(services, httpClientSettings);
        ConfigureGlobalHttpClientSettings(services, httpClientSettings);

        // Add HttpContext accessor
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Configures AI provider HTTP client with dynamic timeout from environment/config
    /// </summary>
    private static void ConfigureAIProviderClient(IServiceCollection services, IConfiguration configuration, HttpClientSettings settings)
    {
        services.AddHttpClient(AIProviderClientName, client =>
        {
            var timeout = GetAIProviderTimeout(configuration, settings);
            client.Timeout = timeout;
            client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = settings.AIProviderMaxConnections,
            UseCookies = false
        })
        .SetHandlerLifetime(settings.HandlerLifetime);
    }

    /// <summary>
    /// Configures portfolio service HTTP client
    /// </summary>
    private static void ConfigurePortfolioServiceClient(IServiceCollection services, HttpClientSettings settings)
    {
        services.AddHttpClient(PortfolioServiceClientName, client =>
        {
            client.Timeout = settings.PortfolioServiceTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = settings.PortfolioServiceMaxConnections,
            UseCookies = false
        })
        .SetHandlerLifetime(settings.HandlerLifetime);
    }

    /// <summary>
    /// Configures user service HTTP client
    /// </summary>
    private static void ConfigureUserServiceClient(IServiceCollection services, HttpClientSettings settings)
    {
        services.AddHttpClient(UserServiceClientName, client =>
        {
            client.Timeout = settings.UserServiceTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = settings.UserServiceMaxConnections,
            UseCookies = false
        })
        .SetHandlerLifetime(settings.HandlerLifetime);
    }

    /// <summary>
    /// Configures global HTTP client factory settings
    /// </summary>
    private static void ConfigureGlobalHttpClientSettings(IServiceCollection services, HttpClientSettings settings)
    {
        services.Configure<HttpClientFactoryOptions>(options =>
        {
            options.HandlerLifetime = settings.HandlerLifetime;
        });

        services.AddHttpClient();
    }

    /// <summary>
    /// Gets AI provider timeout from environment variables or configuration
    /// </summary>
    private static TimeSpan GetAIProviderTimeout(IConfiguration configuration, HttpClientSettings settings)
    {
        var timeoutEnv = Environment.GetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS");
        var timeoutCfg = configuration["OpenRouter:TimeoutSeconds"];

        if (int.TryParse(timeoutEnv, out var envSeconds) && envSeconds > 0)
        {
            return TimeSpan.FromSeconds(envSeconds);
        }
        
        if (int.TryParse(timeoutCfg, out var cfgSeconds) && cfgSeconds > 0)
        {
            return TimeSpan.FromSeconds(cfgSeconds);
        }

        return settings.AIProviderTimeout;
    }

    /// <summary>
    /// Gets HTTP client settings from configuration with fallback to defaults
    /// </summary>
    private static HttpClientSettings GetHttpClientSettings(IConfiguration configuration)
    {
        var settings = new HttpClientSettings();
        configuration.GetSection("HttpClient").Bind(settings);
        return settings;
    }

    /// <summary>
    /// HTTP client configuration settings
    /// </summary>
    private class HttpClientSettings
    {
        public string UserAgent { get; set; } = "AIService/1.0";
        public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(5);
        
        // AI Provider settings
        public TimeSpan AIProviderTimeout { get; set; } = TimeSpan.FromMinutes(2);
        public int AIProviderMaxConnections { get; set; } = 5;
        
        // Portfolio Service settings
        public TimeSpan PortfolioServiceTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public int PortfolioServiceMaxConnections { get; set; } = 10;
        
        // User Service settings
        public TimeSpan UserServiceTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public int UserServiceMaxConnections { get; set; } = 10;
    }
}
