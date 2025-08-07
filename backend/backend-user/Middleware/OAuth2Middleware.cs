using backend_user.Services.Abstractions;
using System.Security.Claims;

namespace backend_user.Middleware
{
    /// <summary>
    /// Middleware for OAuth 2.0 authentication using access tokens.
    /// </summary>
    public class OAuth2Middleware
    {
        private readonly RequestDelegate _next;

        public OAuth2Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOAuth2Service oauth2Service)
        {
            // Add security headers to all responses
            context.Response.OnStarting(() => {
                context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                if (!context.Response.Headers.ContainsKey("Content-Type"))
                {
                    context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
                }
                // Prevent caching of sensitive API responses
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
                return Task.CompletedTask;
            });

            // Skip auth for certain paths
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (
                path.StartsWith("/api/users/login") ||
                path.StartsWith("/api/users/register") ||
                path.StartsWith("/api/users/oauth-providers/check") ||
                path.StartsWith("/api/users/check-email") ||
                path.Contains("/api/users/email/") ||  // Allow getUserByEmail during auth
                (path.Contains("/oauth-providers/") && context.Request.Method == "GET") ||  // Allow OAuth provider lookup during auth
                path.StartsWith("/api/oauth/") ||
                path.StartsWith("/api/oauth2/") ||
                path.StartsWith("/openapi") ||
                path.StartsWith("/swagger") ||
                path == "/" ||
                path.StartsWith("/health") ||
                // Allow public access to user portfolio info for portfolio display
                (path.Contains("/api/users/") && path.Contains("/portfolio-info") && context.Request.Method == "GET") ||
                // Allow public access to user information for portfolio cards
                (path.Contains("/api/users/") && context.Request.Method == "GET" && !path.Contains("/oauth-providers/") && !path.Contains("/bookmarks/"))))
            {
                await _next(context);
                return;
            }

            // Extract access token from Authorization header
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing or invalid Authorization header");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Empty access token");
                return;
            }

            // Validate the access token
            var user = await oauth2Service.GetUserByAccessTokenAsync(token);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid or expired access token");
                return;
            }

            // Add user information to the context
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Username),
                new("IsAdmin", user.IsAdmin.ToString())
            };

            var identity = new ClaimsIdentity(claims, "OAuth2");
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;

            await _next(context);
        }
    }
}