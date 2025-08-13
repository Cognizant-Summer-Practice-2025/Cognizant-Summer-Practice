namespace backend_AI.Config;

/// <summary>
/// Configuration class for API documentation setup 
/// </summary>
public static class ApiDocumentationConfiguration
{
    /// <summary>
    /// Configures API documentation services (OpenAPI/Swagger)
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        
        return services;
    }
}
