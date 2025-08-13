using Microsoft.AspNetCore.Http;

namespace backend_AI.Services.Abstractions
{
    /// <summary>
    /// Applies security headers to HTTP responses.
    /// </summary>
    public interface ISecurityHeadersService
    {
        void ApplySecurityHeaders(HttpContext context);
    }
}


