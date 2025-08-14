using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Middleware
{
    /// <summary>
    /// Authentication middleware using 
    /// Delegates security headers, path authorization, and authentication to services.
    /// </summary>
    public class OAuth2Middleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OAuth2Middleware> _logger;

        public OAuth2Middleware(RequestDelegate next, ILogger<OAuth2Middleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ISecurityHeadersService securityHeadersService,
            IAuthorizationPathService authorizationPathService,
            IAuthenticationContextService authenticationContextService)
        {
            try
            {
                // Apply security headers
                securityHeadersService.ApplySecurityHeaders(context);

                var path = context.Request.Path.Value?.ToLower();
                var method = context.Request.Method;
                _logger.LogInformation("🔐 Middleware: Processing request {Method} {Path}", method, path);

                // Check if authentication is required
                if (!authorizationPathService.RequiresAuthentication(context))
                {
                    _logger.LogInformation("🔐 Middleware: Skipping auth for public endpoint {Method} {Path}", method, path);
                    await _next(context);
                    return;
                }

                // Authenticate using strategies
                var principal = await authenticationContextService.AuthenticateAsync(context);
                if (principal == null)
                {
                    _logger.LogWarning("🔐 Middleware: Authentication failed for {Method} {Path}", method, path);
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\": \"Unauthorized: Authentication failed\"}");
                    }
                    return;
                }

                context.User = principal;
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔐 Middleware: Error processing request {Method} {Path}", context.Request.Method, context.Request.Path);
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Unauthorized: Internal authentication error\"}");
                }
            }
        }
    }
}