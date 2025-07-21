using backend_user.DTO;
using backend_user.Models;

namespace backend_user.Services
{
    /// <summary>
    /// Interface for user authentication operations.
    /// Follows Interface Segregation Principle by separating auth concerns from user management.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user using OAuth provider credentials.
        /// </summary>
        /// <param name="provider">The OAuth provider type</param>
        /// <param name="providerId">The provider-specific user ID</param>
        /// <param name="providerEmail">The email from the OAuth provider</param>
        /// <returns>The authenticated user or null if authentication fails</returns>
        Task<User?> AuthenticateOAuthUserAsync(OAuthProviderType provider, string providerId, string providerEmail);
        
        /// <summary>
        /// Updates the last login timestamp for a user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateLastLoginAsync(Guid userId);
        
        /// <summary>
        /// Validates if an OAuth provider is linked to a user.
        /// </summary>
        /// <param name="provider">The OAuth provider type</param>
        /// <param name="providerId">The provider-specific user ID</param>
        /// <returns>True if the provider is linked to a user, false otherwise</returns>
        Task<bool> IsOAuthProviderLinkedAsync(OAuthProviderType provider, string providerId);
    }
}
