using backend_user.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace backend_user.Middleware
{
    /// <summary>
    /// Middleware for authentication 
    /// </summary>
    public class OAuth2Middleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OAuth2Middleware> _logger;

        public OAuth2Middleware(RequestDelegate next, ILogger<OAuth2Middleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes the HTTP request through the authentication pipeline.
        /// </summary>
        public async Task InvokeAsync(
            HttpContext context,
            ISecurityHeadersService securityHeadersService,
            IAuthorizationPathService authorizationPathService,
            IAuthenticationContextService authenticationService,
            IClaimsBuilderService claimsBuilderService)
        {
            try
            {
                // Apply security headers to all responses 
                securityHeadersService.ApplySecurityHeaders(context);

                // Check if authentication is required for this path
                if (!authorizationPathService.RequiresAuthentication(context))
                {
                    _logger.LogDebug("Skipping authentication for public path: {Path}", context.Request.Path);
                    await _next(context);
                    return;
                }

                // Attempt authentication using strategy pattern  
                var user = await authenticationService.AuthenticateAsync(context);
                if (user == null)
                {
                    _logger.LogWarning("Authentication failed for path: {Path}", context.Request.Path);
                    await WriteUnauthorizedResponse(context, "Unauthorized: Authentication failed");
                    return;
                }

                // Build user claims and set principal 
                var principal = claimsBuilderService.BuildPrincipal(user);
                context.User = principal;

                _logger.LogDebug("Successfully authenticated user {UserId} for path {Path}", 
                    user.Id, context.Request.Path);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in OAuth2Middleware for path: {Path}", context.Request.Path);
                await WriteUnauthorizedResponse(context, "Unauthorized: Internal authentication error");
            }
        }

        /// <summary>
        /// Writes an unauthorized response to the HTTP context.
        /// </summary>
        private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\": \"{message}\"}}");
            }
        }
    }
}