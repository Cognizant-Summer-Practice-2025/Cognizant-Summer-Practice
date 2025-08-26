using Microsoft.Extensions.Http;

namespace backend_portfolio.Config;

/// <summary>
/// Configuration class for HTTP client services 
/// </summary>
public static class HttpClientConfiguration
{
    public const string ExternalUserServiceClientName = "ExternalUserService";
    public const string VercelApiClientName = "VercelApi";
    public const string HomePortfolioServiceClientName = "HomePortfolioService";

    /// <summary>
    /// Configures HTTP client services with connection pooling and recycling
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var httpClientSettings = GetHttpClientSettings(configuration);

        // Configure named HttpClient for external user service
        services.AddHttpClient(ExternalUserServiceClientName, client =>
        {
            client.Timeout = httpClientSettings.Timeout;
            client.DefaultRequestHeaders.Add("User-Agent", httpClientSettings.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = httpClientSettings.MaxConnectionsPerServer,
            UseCookies = httpClientSettings.UseCookies
        })
        .SetHandlerLifetime(httpClientSettings.HandlerLifetime);

        // Configure named HttpClient for Vercel API
        services.AddHttpClient(VercelApiClientName, client =>
        {
            client.BaseAddress = new Uri(configuration["Vercel:ApiUrl"] ?? "https://api.vercel.com");
            client.Timeout = TimeSpan.FromMinutes(5); // Longer timeout for deployments
            client.DefaultRequestHeaders.Add("User-Agent", "Goalkeeper-Portfolio-Deployer/1.0");
            
            var vercelToken = configuration["Vercel:Token"];
            if (!string.IsNullOrEmpty(vercelToken))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vercelToken);
            }
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = 5,
            UseCookies = false
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(10));

        // Configure named HttpClient for Home Portfolio Service
        services.AddHttpClient(HomePortfolioServiceClientName, client =>
        {
            client.BaseAddress = new Uri(configuration["HomePortfolioService:BaseUrl"] ?? "http://localhost:3002");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Goalkeeper-Template-Extractor/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = 5,
            UseCookies = false
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Configure HttpClient factory with global pooling settings
        services.Configure<HttpClientFactoryOptions>(options =>
        {
            options.HandlerLifetime = httpClientSettings.HandlerLifetime;
        });

        // Configure default HttpClient factory for any other HTTP needs
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Gets HTTP client settings from configuration with fallback to defaults
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <returns>HTTP client settings</returns>
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
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public string UserAgent { get; set; } = "PortfolioService/1.0";
        public int MaxConnectionsPerServer { get; set; } = 10;
        public bool UseCookies { get; set; } = false;
        public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(5);
    }
}
