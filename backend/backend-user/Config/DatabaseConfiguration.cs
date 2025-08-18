using Microsoft.EntityFrameworkCore;
using backend_user.Data;
using backend_user.Models;
using Npgsql;

namespace backend_user.Config;

/// <summary>
/// Configuration class for database-related services 
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configures database services including DbContext and data source with enum mapping
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");

        var dataSource = CreateDataSourceWithEnumMapping(connectionString);

        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(dataSource)
                   .UseSnakeCaseNamingConvention());

        services.AddSingleton(dataSource);

        return services;
    }

    /// <summary>
    /// Creates Npgsql data source with enum type mappings
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>Configured NpgsqlDataSource</returns>
    private static NpgsqlDataSource CreateDataSourceWithEnumMapping(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        
        // Map custom enum types used in the database
        dataSourceBuilder.MapEnum<OAuthProviderType>("oauth_provider_type");
        
        return dataSourceBuilder.Build();
    }
}

