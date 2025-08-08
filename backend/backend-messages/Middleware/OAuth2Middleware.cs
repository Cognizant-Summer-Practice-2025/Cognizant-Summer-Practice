using BackendMessages.Services.Abstractions;

namespace BackendMessages.Middleware
{
    /// <summary>
    /// Middleware for OAuth 2.0 authentication that validates tokens via the user service.
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
            // Add security headers to all responses
            context.Response.OnStarting(() => {
                context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                return Task.CompletedTask;
            });

            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;
            
            _logger.LogInformation("🔐 Middleware: Processing request {Method} {Path}", method, path);
            
            // Skip auth for certain paths
            if (path != null && (
                path.StartsWith("/swagger") ||
                path == "/" ||
                path.StartsWith("/health") ||
                path.StartsWith("/messagehub") ||  // Allow SignalR hub connection
                // Allow test endpoint for service health checks
                (path.StartsWith("/api/users/test") && method == "GET")))
            {
                _logger.LogInformation("🔐 Middleware: Skipping auth for public endpoint {Method} {Path}", method, path);
                await _next(context);
                return;
            }

            // Extract access token from Authorization header
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            _logger.LogInformation("🔐 Middleware: Authorization header present: {HasHeader}, Value: {HeaderValue}", 
                !string.IsNullOrEmpty(authHeader), 
                authHeader?.Substring(0, Math.Min(20, authHeader?.Length ?? 0)) + "..." ?? "null");
            
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("🔐 Middleware: Unauthorized request to {Method} {Path}: Missing or invalid Authorization header", method, path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing or invalid Authorization header");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            _logger.LogInformation("🔐 Middleware: Extracted token length: {TokenLength}", token.Length);
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("🔐 Middleware: Unauthorized request to {Method} {Path}: Empty access token", method, path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Empty access token");
                return;
            }

            // Validate the access token via user service
            _logger.LogInformation("🔐 Middleware: Validating token with user service...");
            var principal = await userAuthService.ValidateTokenAsync(token);
            if (principal == null)
            {
                _logger.LogWarning("🔐 Middleware: Unauthorized request to {Method} {Path}: Invalid or expired access token", method, path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid or expired access token");
                return;
            }

            // Add user information to the context
            _logger.LogInformation("🔐 Middleware: Token validated successfully for user: {UserId}", 
                principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            context.User = principal;

            await _next(context);
        }
    }
}
