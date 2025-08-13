using backend_user.Services.Abstractions;

namespace backend_user.Services
{
    /// <summary>
    /// Service responsible for applying security headers to HTTP responses.
    /// </summary>
    public class SecurityHeadersService : ISecurityHeadersService
    {
        /// <summary>
        /// Applies security headers to prevent common security vulnerabilities.
        /// </summary>
        /// <param name="context">The HTTP context containing the response to modify.</param>
        public void ApplySecurityHeaders(HttpContext context)
        {
            if (context?.Response == null)
            {
                return;
            }

            context.Response.OnStarting(() =>
            {
                // Cross-Origin Resource Policy
                context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
                
                // Prevent MIME-type sniffing
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                
                // Set default content type if not already set
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
        }
    }
}
