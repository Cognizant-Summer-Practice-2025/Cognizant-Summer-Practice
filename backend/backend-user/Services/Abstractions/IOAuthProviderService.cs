using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;
using backend_user.Models;

namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Interface for OAuth provider management operations.
    /// </summary>
    public interface IOAuthProviderService
    {
        /// <summary>
        /// Gets all OAuth providers for a user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>A collection of OAuth provider summary DTOs</returns>
        Task<IEnumerable<OAuthProviderSummaryDto>> GetUserOAuthProvidersAsync(Guid userId);
        
        /// <summary>
        /// Creates a new OAuth provider.
        /// </summary>
        /// <param name="request">The OAuth provider creation request</param>
        /// <returns>The created OAuth provider response DTO</returns>
        Task<OAuthProviderResponseDto> CreateOAuthProviderAsync(OAuthProviderCreateRequestDto request);
        
        /// <summary>
        /// Updates an existing OAuth provider.
        /// </summary>
        /// <param name="id">The OAuth provider ID</param>
        /// <param name="request">The OAuth provider update request</param>
        /// <returns>The updated OAuth provider response DTO or null if not found</returns>
        Task<OAuthProviderResponseDto?> UpdateOAuthProviderAsync(Guid id, OAuthProviderUpdateRequestDto request);
        
        /// <summary>
        /// Deletes an OAuth provider.
        /// </summary>
        /// <param name="id">The OAuth provider ID</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> DeleteOAuthProviderAsync(Guid id);
        
        /// <summary>
        /// Checks if an OAuth provider exists.
        /// </summary>
        /// <param name="provider">The OAuth provider type</param>
        /// <param name="providerId">The provider-specific user ID</param>
        /// <returns>An object indicating existence and provider details</returns>
        Task<object> CheckOAuthProviderAsync(OAuthProviderType provider, string providerId);
        
        /// <summary>
        /// Gets a specific OAuth provider for a user by provider type.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="provider">The OAuth provider type</param>
        /// <returns>An object indicating existence and provider details</returns>
        Task<object> GetUserOAuthProviderByTypeAsync(Guid userId, OAuthProviderType provider);
    }
}
