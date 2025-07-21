using backend_user.DTO;
using backend_user.Models;

namespace backend_user.Services
{
    /// <summary>
    /// Interface for login operations.
    /// Follows Interface Segregation Principle by separating login concerns from other auth operations.
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Logs in a user using OAuth provider credentials.
        /// </summary>
        /// <param name="request">The OAuth login request</param>
        /// <returns>Login response containing user information</returns>
        Task<LoginResponseDto> LoginWithOAuthAsync(OAuthLoginRequestDto request);
        
        /// <summary>
        /// Validates OAuth credentials without performing login.
        /// </summary>
        /// <param name="provider">The OAuth provider type</param>
        /// <param name="providerId">The provider-specific user ID</param>
        /// <param name="providerEmail">The email from the OAuth provider</param>
        /// <returns>True if credentials are valid, false otherwise</returns>
        Task<bool> ValidateOAuthCredentialsAsync(OAuthProviderType provider, string providerId, string providerEmail);
        
        /// <summary>
        /// Updates OAuth token information during login.
        /// </summary>
        /// <param name="provider">The OAuth provider type</param>
        /// <param name="providerId">The provider-specific user ID</param>
        /// <param name="accessToken">The new access token</param>
        /// <param name="refreshToken">The new refresh token (optional)</param>
        /// <param name="tokenExpiresAt">Token expiration time (optional)</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateOAuthTokenAsync(OAuthProviderType provider, string providerId, string accessToken, string? refreshToken = null, DateTime? tokenExpiresAt = null);
    }
}
