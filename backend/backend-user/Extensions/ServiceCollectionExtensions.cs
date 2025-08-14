using backend_user.Services;
using backend_user.Services.Abstractions;

namespace backend_user.Extensions
{
    /// <summary>
    /// Extension methods for registering authentication services in the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all authentication-related services
        /// </summary>
        /// <param name="services">The service collection to register services in.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            // Register core services 
            services.AddScoped<ISecurityHeadersService, SecurityHeadersService>();
            services.AddScoped<IAuthorizationPathService, AuthorizationPathService>();
            services.AddScoped<IClaimsBuilderService, ClaimsBuilderService>();
            
            // Register authentication strategies 
            services.AddScoped<IAuthenticationStrategy, OAuth2AuthenticationStrategy>();

            // Register authentication context service
            services.AddScoped<IAuthenticationContextService, AuthenticationContextService>();

            return services;
        }

        /// <summary>
        /// Registers additional authentication strategies.
        /// Demonstrates Open/Closed Principle - easily extensible for new authentication methods.
        /// </summary>
        /// <param name="services">The service collection to register services in.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddCustomAuthenticationStrategy<TStrategy>(this IServiceCollection services)
            where TStrategy : class, IAuthenticationStrategy
        {
            services.AddScoped<IAuthenticationStrategy, TStrategy>();
            return services;
        }
    }
}
