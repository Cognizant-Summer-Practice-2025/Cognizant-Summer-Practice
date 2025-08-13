using backend_user.Models;

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
        /// <returns>The authenticated user if successful, null otherwise.</returns>
        Task<User?> AuthenticateAsync(HttpContext context);
    }
}
