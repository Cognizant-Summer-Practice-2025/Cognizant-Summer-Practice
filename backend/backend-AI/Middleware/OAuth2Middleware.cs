using backend_AI.Services.Abstractions;

namespace backend_AI.Middleware
{
    /// <summary>
    /// OAuth2 authentication middleware for the AI backend, mirroring the portfolio service behavior.
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
            // Security headers
            securityHeadersService.ApplySecurityHeaders(context);

            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;
            _logger.LogInformation("🔐 AI Middleware: Processing {Method} {Path}", method, path);

            // Public endpoints to skip
            if (!authorizationPathService.RequiresAuthentication(context))
            {
                _logger.LogInformation("🔐 AI Middleware: Skipping auth for public endpoint {Method} {Path}", method, path);
                await _next(context);
                return;
            }

            try
            {
                var principal = await authenticationContextService.AuthenticateAsync(context);
                if (principal == null)
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\": \"Unauthorized\"}");
                    }
                    return;
                }

                context.User = principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔐 AI Middleware: Error during authentication");
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Unauthorized: Internal authentication error\"}");
                }
                return;
            }

            await _next(context);
        }
    }
}


