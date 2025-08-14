using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Strategy interface for different authentication methods.
    /// Follows Strategy pattern and Interface Segregation Principle.
    /// </summary>
    public interface IAuthenticationStrategy
    {
        /// <summary>
        /// Authenticates a request and returns user information if successful.
        /// </summary>
        /// <param name="context">The HTTP context containing request information.</param>
        /// <returns>A ClaimsPrincipal if authentication succeeds, null otherwise.</returns>
        Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context);

        /// <summary>
        /// Determines if this strategy can handle the given request.
        /// </summary>
        /// <param name="context">The HTTP context to evaluate.</param>
        /// <returns>True if this strategy can handle the request, false otherwise.</returns>
        bool CanHandle(HttpContext context);
    }
}
