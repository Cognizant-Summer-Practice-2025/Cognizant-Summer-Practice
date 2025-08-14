using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend_portfolio.Services.Abstractions
{
    /// <summary>
    /// Strategy interface for different authentication methods.
    /// </summary>
    public interface IAuthenticationStrategy
    {
        /// <summary>
        /// Determines if this strategy can handle the given request.
        /// </summary>
        /// <param name="context">The HTTP context to evaluate.</param>
        /// <returns>True if this strategy can handle the request, false otherwise.</returns>
        bool CanHandle(HttpContext context);

        /// <summary>
        /// Attempts to authenticate the request.
        /// </summary>
        /// <param name="context">The HTTP context to authenticate.</param>
        /// <returns>A ClaimsPrincipal if authentication succeeds, null otherwise.</returns>
        Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context);
    }
}


