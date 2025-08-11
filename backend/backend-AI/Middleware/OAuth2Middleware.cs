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

        public async Task InvokeAsync(HttpContext context, IUserAuthenticationService userAuthService)
        {
            // Security headers
            context.Response.OnStarting(() => {
                context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                return Task.CompletedTask;
            });

            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;
            _logger.LogInformation("🔐 AI Middleware: Processing {Method} {Path}", method, path);

            // Public endpoints to skip
            if (path != null && (
                path.StartsWith("/openapi") ||
                path.StartsWith("/swagger") ||
                path == "/" ||
                path.StartsWith("/health") ||
                // Allow GET for generate endpoints during testing? Remove if you want all protected
                false
            ))
            {
                _logger.LogInformation("🔐 AI Middleware: Skipping auth for public endpoint {Method} {Path}", method, path);
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            _logger.LogInformation("🔐 AI Middleware: Authorization header present: {HasHeader}, Value: {HeaderPreview}",
                !string.IsNullOrEmpty(authHeader),
                authHeader?.Substring(0, Math.Min(20, authHeader?.Length ?? 0)) + "..." ?? "null");

            if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("🔐 AI Middleware: Missing or invalid Authorization header");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing or invalid Authorization header");
                return;
            }

            var token = authHeader.Substring(7).Trim();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("🔐 AI Middleware: Empty access token");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Empty access token");
                return;
            }

            try
            {
                var principal = await userAuthService.ValidateTokenAsync(token);
                if (principal == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized: Invalid or expired access token");
                    return;
                }
                context.User = principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔐 AI Middleware: Error validating token");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Token validation failed");
                return;
            }

            await _next(context);
        }
    }
}


