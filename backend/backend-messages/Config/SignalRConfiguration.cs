using BackendMessages.Hubs;
using Microsoft.AspNetCore.Http.Connections;

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
        services.AddSignalR(options =>
        {
            // Production-ready configuration
            options.EnableDetailedErrors = true; // Enable for debugging, disable in production
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
            options.StreamBufferCapacity = 10;
        });
        return services;
    }

    /// <summary>
    /// Maps SignalR hubs to the application
    /// </summary>
    /// <param name="app">Web application to configure</param>
    /// <returns>Configured web application</returns>
    public static WebApplication MapSignalRHubs(this WebApplication app)
    {
        app.MapHub<MessageHub>("/messagehub", options =>
        {
            // Force WebSocket-only transport for production reliability
            options.Transports = HttpTransportType.WebSockets;
            
            // Additional production settings
            options.ApplicationMaxBufferSize = 64 * 1024; // 64KB
            options.TransportMaxBufferSize = 64 * 1024; // 64KB
        });
        return app;
    }
}
