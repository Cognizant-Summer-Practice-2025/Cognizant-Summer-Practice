using backend_user.Models;

namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Interface for simplified OAuth 2.0 service using existing oauth_providers table.
    /// </summary>
    public interface IOAuth2Service
    {
        /// <summary>
        /// Validates an OAuth provider's access token.
        /// </summary>
        /// <param name="token">The access token to validate.</param>
        /// <returns>The OAuth provider if token is valid, null otherwise.</returns>
        Task<OAuthProvider?> ValidateAccessTokenAsync(string token);

        /// <summary>
        /// Gets user by access token.
        /// </summary>
        /// <param name="token">The access token.</param>
        /// <returns>The user if token is valid, null otherwise.</returns>
        Task<User?> GetUserByAccessTokenAsync(string token);

        /// <summary>
        /// Refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>Updated OAuth provider data if refresh was successful, null otherwise.</returns>
        Task<OAuthProvider?> RefreshAccessTokenAsync(string refreshToken);
    }
}