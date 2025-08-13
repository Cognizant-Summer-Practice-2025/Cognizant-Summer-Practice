using Microsoft.AspNetCore.Http;

namespace backend_portfolio.Services.Abstractions
{
    /// <summary>
    /// Interface for configuring security headers on HTTP responses.
    /// </summary>
    public interface ISecurityHeadersService
    {
        /// <summary>
        /// Applies security headers to the HTTP response.
        /// </summary>
        /// <param name="context">The HTTP context containing the response to modify.</param>
        void ApplySecurityHeaders(HttpContext context);
    }
}


