using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend_AI.Services.Abstractions
{
    /// <summary>
    /// Strategy interface for different authentication mechanisms.
    /// </summary>
    public interface IAuthenticationStrategy
    {
        /// <summary>
        /// Determines whether this strategy can handle the given request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if it can handle; otherwise false.</returns>
        bool CanHandle(HttpContext context);

        /// <summary>
        /// Attempts to authenticate the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A ClaimsPrincipal if authenticated; otherwise null.</returns>
        Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context);
    }
}


