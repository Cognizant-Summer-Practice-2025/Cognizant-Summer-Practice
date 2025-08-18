using Microsoft.EntityFrameworkCore;
using backend_portfolio.Data;
using Npgsql;

namespace backend_portfolio.Config;

/// <summary>
/// Configuration class for database-related services 
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configures database services including DbContext and data source
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database_Portfolio")
            ?? throw new InvalidOperationException("Database connection string 'Database_Portfolio' is not configured.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseNpgsql(dataSource)
                   .UseSnakeCaseNamingConvention());

        services.AddSingleton(dataSource);

        return services;
    }
}
