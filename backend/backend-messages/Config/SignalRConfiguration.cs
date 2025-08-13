using BackendMessages.Hubs;

namespace BackendMessages.Config;

/// <summary>
/// Configuration class for SignalR setup 
/// </summary>
public static class SignalRConfiguration
{
    /// <summary>
    /// Configures SignalR services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSignalRConfiguration(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

    /// <summary>
    /// Maps SignalR hubs to the application
    /// </summary>
    /// <param name="app">Web application to configure</param>
    /// <returns>Configured web application</returns>
    public static WebApplication MapSignalRHubs(this WebApplication app)
    {
        app.MapHub<MessageHub>("/messagehub");
        return app;
    }
}
