using backend_user.Middleware;

namespace backend_user.Config;

/// <summary>
/// Configuration class for middleware pipeline setup 
/// </summary>
public static class MiddlewareConfiguration
{
    /// <summary>
    /// Configures the middleware pipeline for the application
    /// </summary>
    /// <param name="app">Web application to configure</param>
    /// <returns>Configured web application</returns>
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.ConfigureDevelopmentMiddleware();
        }

        app.UseHttpsRedirection();
        app.UseCors(CorsConfiguration.FrontendPolicyName);
        app.UseMiddleware<OAuth2Middleware>();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Configures middleware specific to development environment
    /// </summary>
    /// <param name="app">Web application to configure</param>
    /// <returns>Configured web application</returns>
    private static WebApplication ConfigureDevelopmentMiddleware(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "User API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
