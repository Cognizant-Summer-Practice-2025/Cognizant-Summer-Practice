using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend_AI.Services.Abstractions
{
    /// <summary>
    /// Coordinates authentication using a set of strategies.
    /// </summary>
    public interface IAuthenticationContextService
    {
        /// <summary>
        /// Attempts to authenticate the request using the first capable strategy.
        /// </summary>
        /// <param name="context">HTTP context.</param>
        /// <returns>Authenticated principal or null.</returns>
        Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context);
    }
}


