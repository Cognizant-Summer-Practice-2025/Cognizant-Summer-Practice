using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;

namespace BackendMessages.Config;

/// <summary>
/// Configuration class for database-related services 
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configures database services including DbContext
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MessagesDatabase")
            ?? throw new InvalidOperationException("Database connection string 'MessagesDatabase' is not configured.");

        services.AddDbContext<MessagesDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
