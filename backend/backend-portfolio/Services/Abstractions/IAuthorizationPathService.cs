using Microsoft.AspNetCore.Http;

namespace backend_portfolio.Services.Abstractions
{
    /// <summary>
    /// Interface for determining if a request path requires authentication.
    /// </summary>
    public interface IAuthorizationPathService
    {
        /// <summary>
        /// Determines if the given HTTP context requires authentication.
        /// </summary>
        /// <param name="context">The HTTP context to evaluate.</param>
        /// <returns>True if authentication is required, false if the path is public.</returns>
        bool RequiresAuthentication(HttpContext context);
    }
}


