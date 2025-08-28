using Microsoft.Extensions.Http;

namespace BackendMessages.Config;

/// <summary>
/// Configuration class for HTTP client services 
/// </summary>
public static class HttpClientConfiguration
{
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
        var userServiceBaseUrl =
            Environment.GetEnvironmentVariable("USER_SERVICE_URL")
            ?? configuration["UserService:BaseUrl"]
            ?? configuration["UserServiceUrl"]
            ?? "http://localhost:5200";

        // Configure named HttpClient for user service
        services.AddHttpClient(UserServiceClientName, client =>
        {
            client.Timeout = httpClientSettings.Timeout;
            client.DefaultRequestHeaders.Add("User-Agent", httpClientSettings.UserAgent);
            if (Uri.TryCreate(userServiceBaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            MaxConnectionsPerServer = httpClientSettings.MaxConnectionsPerServer,
            UseCookies = httpClientSettings.UseCookies
        })
        .SetHandlerLifetime(httpClientSettings.HandlerLifetime);

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
        public string UserAgent { get; set; } = "MessagesService/1.0";
        public int MaxConnectionsPerServer { get; set; } = 10;
        public bool UseCookies { get; set; } = false;
        public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(5);
    }
}
