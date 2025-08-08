using System.Security.Claims;

namespace BackendMessages.Services.Abstractions
{
    /// <summary>
    /// Service for validating user authentication tokens.
    /// </summary>
    public interface IUserAuthenticationService
    {
        /// <summary>
        /// Validates an access token by calling the user service.
        /// </summary>
        /// <param name="token">The access token to validate.</param>
        /// <returns>The user claims if token is valid, null otherwise.</returns>
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    }
}
