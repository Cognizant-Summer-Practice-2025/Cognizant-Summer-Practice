using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend_portfolio.Services.Abstractions
{
    /// <summary>
    /// Interface for coordinating authentication strategies.
    /// Acts as the context
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


