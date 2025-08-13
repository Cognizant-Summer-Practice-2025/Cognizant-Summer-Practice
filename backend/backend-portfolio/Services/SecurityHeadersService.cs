using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Services
{
    /// <summary>
    /// Service responsible for applying security headers to HTTP responses.
    /// </summary>
    public class SecurityHeadersService : ISecurityHeadersService
    {
        public void ApplySecurityHeaders(HttpContext context)
        {
            if (context?.Response == null)
            {
                return;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                if (!context.Response.Headers.ContainsKey("Content-Type"))
                {
                    context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
                }

                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";

                return Task.CompletedTask;
            });
        }
    }
}


