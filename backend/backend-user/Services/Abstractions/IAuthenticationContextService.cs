using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Interface for coordinating authentication strategies.
    /// Follows Strategy pattern as the context that manages authentication strategies.
    /// </summary>
    public interface IAuthenticationContextService
    {
        /// <summary>
        /// Attempts to authenticate the request using available strategies.
        /// </summary>
        /// <param name="context">The HTTP context to authenticate.</param>
        /// <returns>A ClaimsPrincipal if authentication succeeds, null otherwise.</returns>
        Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context);
    }
}
