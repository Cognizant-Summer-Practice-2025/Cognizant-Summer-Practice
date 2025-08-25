using System.Text.Json.Serialization;

namespace backend_portfolio.Config;

/// <summary>
/// Configuration class for JSON serialization settings 
/// </summary>
public static class JsonConfiguration
{
    /// <summary>
    /// Configures JSON serialization options for controllers
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                // Removed camelCase policy to preserve original property names
                options.JsonSerializerOptions.WriteIndented = false;
            });

        return services;
    }
}
